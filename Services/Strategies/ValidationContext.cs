using TechMoveLogisticsApplication.Models;

namespace TechMoveLogisticsApplication.Services.Strategies;

public class ValidationContext
{
    private readonly IEnumerable<IValidationStrategy> _strategies;

    public ValidationContext(IEnumerable<IValidationStrategy> strategies)
    {
        _strategies = strategies;
    }

    public BusinessValidationResult ValidateRequest(Contract contract, ServiceRequest request)
    {
        var combined = new BusinessValidationResult();

        foreach (var strategy in _strategies)
        {
            var result = strategy.Validate(contract, request);
            foreach (var error in result.Errors)
            {
                combined.AddError(error);
            }
        }

        return combined;
    }
}
