using Microsoft.EntityFrameworkCore;
using TechMoveLogisticsApplication.Data;
using TechMoveLogisticsApplication.Models;
using TechMoveLogisticsApplication.Services;
using TechMoveLogisticsApplication.Services.Currency;
using TechMoveLogisticsApplication.Services.Strategies;
using TechMoveLogisticsApplication.Services.Workflow;
using TechMoveLogisticsApplication.ViewModels;

namespace TechMoveLogisticsApplication.Tests;

public class ServiceRequestWorkflowServiceTests
{
    [Fact]
    public async Task CreateApprovedRequestAsync_ApprovesRequestCreatesInvoiceAndAudit()
    {
        await using var context = CreateContext();
        var contract = await SeedContractAsync(context, ContractStatus.Active, DateTime.Today.AddDays(-2), DateTime.Today.AddMonths(2));
        var service = CreateService(context, new FakeCurrencyConversionService(20.25m));

        var result = await service.CreateApprovedRequestAsync(new ServiceRequestCreateViewModel
        {
            ContractId = contract.ContractId,
            RequestType = "Customs clearance",
            Description = "Clear one inbound container",
            RequestedAmountUsd = 10
        });

        var request = await context.ServiceRequests.Include(item => item.Invoice).SingleAsync();
        var auditLog = await context.AuditLogs.SingleAsync(log => log.EventType == "Service Request Approved");

        Assert.True(result.Succeeded);
        Assert.Equal(ServiceRequestStatus.Approved, request.Status);
        Assert.Equal(20.25m, request.ExchangeRate);
        Assert.Equal(202.50m, request.Cost);
        Assert.NotNull(request.Invoice);
        Assert.Equal(202.50m, request.Invoice.AmountZar);
        Assert.Equal(request.ServiceRequestId, auditLog.ServiceRequestId);
    }

    [Fact]
    public async Task CreateApprovedRequestAsync_DoesNotCreateInvoiceForInvalidContract()
    {
        await using var context = CreateContext();
        var contract = await SeedContractAsync(context, ContractStatus.Expired, DateTime.Today.AddMonths(-3), DateTime.Today.AddDays(-1));
        var currencyService = new FakeCurrencyConversionService(20.25m);
        var service = CreateService(context, currencyService);

        var result = await service.CreateApprovedRequestAsync(new ServiceRequestCreateViewModel
        {
            ContractId = contract.ContractId,
            RequestType = "Customs clearance",
            Description = "Clear one inbound container",
            RequestedAmountUsd = 10
        });

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, error => error.Message.Contains("active contracts"));
        Assert.Equal(0, currencyService.RateCallCount);
        Assert.Empty(context.ServiceRequests);
        Assert.Empty(context.Invoices);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static async Task<Contract> SeedContractAsync(
        ApplicationDbContext context,
        ContractStatus status,
        DateTime startDate,
        DateTime endDate)
    {
        var client = new Client
        {
            Name = "Test Client",
            ContactDetails = "operations@test.example",
            Region = "Africa"
        };
        var contract = new StandardContract
        {
            Client = client,
            StartDate = startDate,
            EndDate = endDate,
            Status = status,
            ServiceLevel = "Standard SLA"
        };

        context.Contracts.Add(contract);
        await context.SaveChangesAsync();
        return contract;
    }

    private static ServiceRequestWorkflowService CreateService(
        ApplicationDbContext context,
        ICurrencyConversionService currencyConversionService)
    {
        var validationContext = new ValidationContext([
            new ActiveContractValidationStrategy(),
            new SlaValidationStrategy(),
            new InternationalRequestValidationStrategy()
        ]);
        var invoiceService = new InvoiceService([
            new LocalInvoiceStrategy(),
            new InternationalInvoiceStrategy()
        ]);

        return new ServiceRequestWorkflowService(context, currencyConversionService, validationContext, invoiceService);
    }

    private sealed class FakeCurrencyConversionService : ICurrencyConversionService
    {
        private readonly decimal _rate;

        public FakeCurrencyConversionService(decimal rate)
        {
            _rate = rate;
        }

        public int RateCallCount { get; private set; }

        public Task<decimal> GetUsdToZarRateAsync(CancellationToken cancellationToken = default)
        {
            RateCallCount++;
            return Task.FromResult(_rate);
        }

        public decimal ConvertUsdToZar(decimal amountUsd, decimal exchangeRate)
        {
            return Math.Round(amountUsd * exchangeRate, 2, MidpointRounding.AwayFromZero);
        }
    }
}
