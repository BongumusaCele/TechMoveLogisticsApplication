using TechMoveLogisticsApplication.Models;

namespace TechMoveLogisticsApplication.Services.Strategies;

public class ActiveContractValidationStrategy : IValidationStrategy
{
    public BusinessValidationResult Validate(Contract contract, ServiceRequest request)
    {
        var result = new BusinessValidationResult();

        if (contract.Status != ContractStatus.Active)
        {
            result.AddError("Service requests can only be raised against active contracts.");
        }

        var today = DateTime.Today;
        if (contract.StartDate.Date > today || contract.EndDate.Date < today)
        {
            result.AddError("The contract date range is not currently valid.");
        }

        return result;
    }
}
