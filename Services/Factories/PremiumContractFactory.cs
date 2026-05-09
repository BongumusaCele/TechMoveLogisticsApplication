using TechMoveLogisticsApplication.Models;

namespace TechMoveLogisticsApplication.Services.Factories;

public class PremiumContractFactory : IContractFactory
{
    public ContractType ContractType => ContractType.Premium;

    public Contract CreateContract(int clientId, DateTime startDate, DateTime endDate, ContractStatus status, string serviceLevel)
    {
        return new PremiumContract
        {
            ClientId = clientId,
            StartDate = startDate,
            EndDate = endDate,
            Status = status,
            ServiceLevel = serviceLevel,
            PriorityLevel = 1
        };
    }
}
