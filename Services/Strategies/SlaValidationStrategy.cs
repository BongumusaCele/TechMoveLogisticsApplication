using TechMoveLogisticsApplication.Models;

namespace TechMoveLogisticsApplication.Services.Strategies;

public class SlaValidationStrategy : IValidationStrategy
{
    public BusinessValidationResult Validate(Contract contract, ServiceRequest request)
    {
        var result = new BusinessValidationResult();

        if (string.IsNullOrWhiteSpace(contract.ServiceLevel))
        {
            result.AddError("The parent contract must have a service level agreement.");
        }

        if (request.RequestedAmountUsd <= 0)
        {
            result.AddError("The requested amount must be greater than zero.");
        }

        return result;
    }
}
