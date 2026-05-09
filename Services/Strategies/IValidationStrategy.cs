using TechMoveLogisticsApplication.Models;

namespace TechMoveLogisticsApplication.Services.Strategies;

public interface IValidationStrategy
{
    BusinessValidationResult Validate(Contract contract, ServiceRequest request);
}
