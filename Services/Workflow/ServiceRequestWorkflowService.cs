using Microsoft.EntityFrameworkCore;
using TechMoveLogisticsApplication.Data;
using TechMoveLogisticsApplication.Models;
using TechMoveLogisticsApplication.Services.Currency;
using TechMoveLogisticsApplication.Services.Strategies;
using TechMoveLogisticsApplication.ViewModels;

namespace TechMoveLogisticsApplication.Services.Workflow;

public class ServiceRequestWorkflowService : IServiceRequestWorkflowService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrencyConversionService _currencyConversionService;
    private readonly ValidationContext _validationContext;
    private readonly IInvoiceService _invoiceService;

    public ServiceRequestWorkflowService(
        ApplicationDbContext context,
        ICurrencyConversionService currencyConversionService,
        ValidationContext validationContext,
        IInvoiceService invoiceService)
    {
        _context = context;
        _currencyConversionService = currencyConversionService;
        _validationContext = validationContext;
        _invoiceService = invoiceService;
    }

    public async Task<ServiceRequestCreationResult> CreateApprovedRequestAsync(
        ServiceRequestCreateViewModel viewModel,
        CancellationToken cancellationToken = default)
    {
        var result = new ServiceRequestCreationResult();

        var contract = await _context.Contracts
            .Include(item => item.Client)
            .FirstOrDefaultAsync(item => item.ContractId == viewModel.ContractId, cancellationToken);

        if (contract is null)
        {
            result.AddError(nameof(viewModel.ContractId), "Select a valid contract.");
            return result;
        }

        var request = new ServiceRequest
        {
            ContractId = viewModel.ContractId,
            RequestType = viewModel.RequestType,
            Description = viewModel.Description,
            RequestedAmountUsd = viewModel.RequestedAmountUsd,
            CurrencyCode = "USD",
            Status = ServiceRequestStatus.Submitted
        };

        var validation = _validationContext.ValidateRequest(contract, request);
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
            {
                result.AddError(null, error);
            }

            return result;
        }

        var rate = await _currencyConversionService.GetUsdToZarRateAsync(cancellationToken);
        request.ExchangeRate = rate;
        request.Cost = _currencyConversionService.ConvertUsdToZar(viewModel.RequestedAmountUsd, rate);
        ApproveRequest(request);

        _context.ServiceRequests.Add(request);
        await _context.SaveChangesAsync(cancellationToken);

        _context.Invoices.Add(_invoiceService.CreateInvoice(request));
        _context.AuditLogs.Add(new AuditLog
        {
            EventType = "Service Request Approved",
            ContractId = request.ContractId,
            ServiceRequestId = request.ServiceRequestId,
            Message = $"Service request {request.ServiceRequestId} approved and converted at USD/ZAR rate {rate:N4}."
        });
        await _context.SaveChangesAsync(cancellationToken);

        result.MarkCreated(request.ServiceRequestId);
        return result;
    }

    private static void ApproveRequest(ServiceRequest request)
    {
        if (request.Status != ServiceRequestStatus.Submitted)
        {
            throw new InvalidOperationException("Only submitted service requests can be approved.");
        }

        request.Status = ServiceRequestStatus.Approved;
    }
}
