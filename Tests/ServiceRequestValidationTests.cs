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
}
