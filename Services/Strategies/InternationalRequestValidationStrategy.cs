using TechMoveLogisticsApplication.Models;

namespace TechMoveLogisticsApplication.Services.Strategies;

public class InternationalRequestValidationStrategy : IValidationStrategy
{
    public BusinessValidationResult Validate(Contract contract, ServiceRequest request)
    {
        var result = new BusinessValidationResult();

        if (contract is InternationalContract && !string.Equals(request.CurrencyCode, "USD", StringComparison.OrdinalIgnoreCase))
        {
            result.AddError("International contract requests must be captured in USD before conversion to ZAR.");
        }

        return result;
    }
}
