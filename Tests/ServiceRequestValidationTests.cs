using TechMoveLogisticsApplication.Models;
using TechMoveLogisticsApplication.Services.Strategies;

namespace TechMoveLogisticsApplication.Tests;

public class ServiceRequestValidationTests
{
    [Fact]
    public void ValidateRequest_BlocksExpiredContracts()
    {
        var context = new ValidationContext([
            new ActiveContractValidationStrategy(),
            new SlaValidationStrategy()
        ]);
        var contract = new StandardContract
        {
            ClientId = 1,
            StartDate = DateTime.Today.AddYears(-2),
            EndDate = DateTime.Today.AddDays(-1),
            Status = ContractStatus.Expired,
            ServiceLevel = "Standard SLA"
        };
        var request = new ServiceRequest
        {
            ContractId = 1,
            RequestType = "Freight booking",
            Description = "Container pickup",
            RequestedAmountUsd = 100
        };

        var result = context.ValidateRequest(contract, request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Contains("active contracts"));
    }

    [Fact]
    public void ValidateRequest_BlocksContractsOutsideDateRange()
    {
        var context = new ValidationContext([
            new ActiveContractValidationStrategy(),
            new SlaValidationStrategy()
        ]);
        var contract = new StandardContract
        {
            ClientId = 1,
            StartDate = DateTime.Today.AddDays(1),
            EndDate = DateTime.Today.AddMonths(3),
            Status = ContractStatus.Active,
            ServiceLevel = "Future SLA"
        };
        var request = ValidRequest();

        var result = context.ValidateRequest(contract, request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Contains("date range"));
    }

    [Fact]
    public void ValidateRequest_BlocksOnHoldContracts()
    {
        var context = new ValidationContext([
            new ActiveContractValidationStrategy(),
            new SlaValidationStrategy()
        ]);
        var contract = ValidContract();
        contract.Status = ContractStatus.OnHold;

        var result = context.ValidateRequest(contract, ValidRequest());

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Contains("active contracts"));
    }

    [Fact]
    public void ValidateRequest_CollectsMultipleValidationErrors()
    {
        var context = new ValidationContext([
            new ActiveContractValidationStrategy(),
            new SlaValidationStrategy()
        ]);
        var contract = ValidContract();
        contract.ServiceLevel = "";
        var request = ValidRequest();
        request.RequestedAmountUsd = 0;

        var result = context.ValidateRequest(contract, request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Contains("service level"));
        Assert.Contains(result.Errors, error => error.Contains("greater than zero"));
    }

    [Fact]
    public void ValidateRequest_BlocksInternationalRequestsThatAreNotUsd()
    {
        var context = new ValidationContext([
            new InternationalRequestValidationStrategy()
        ]);
        var contract = new InternationalContract
        {
            ClientId = 1,
            StartDate = DateTime.Today.AddDays(-1),
            EndDate = DateTime.Today.AddMonths(3),
            Status = ContractStatus.Active,
            ServiceLevel = "International SLA",
            CurrencyCode = "USD"
        };
        var request = ValidRequest();
        request.CurrencyCode = "EUR";

        var result = context.ValidateRequest(contract, request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Contains("USD"));
    }

    private static StandardContract ValidContract()
    {
        return new StandardContract
        {
            ClientId = 1,
            StartDate = DateTime.Today.AddDays(-1),
            EndDate = DateTime.Today.AddMonths(3),
            Status = ContractStatus.Active,
            ServiceLevel = "Standard SLA"
        };
    }

    private static ServiceRequest ValidRequest()
    {
        return new ServiceRequest
        {
            ContractId = 1,
            RequestType = "Freight booking",
            Description = "Container pickup",
            RequestedAmountUsd = 100,
            CurrencyCode = "USD"
        };
    }
}
