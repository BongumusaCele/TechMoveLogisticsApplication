using TechMoveLogisticsApplication.Models;

namespace TechMoveLogisticsApplication.Services.Factories;

public class StandardContractFactory : IContractFactory
{
    public ContractType ContractType => ContractType.Standard;

    public Contract CreateContract(int clientId, DateTime startDate, DateTime endDate, ContractStatus status, string serviceLevel)
    {
        return new StandardContract
        {
            ClientId = clientId,
            StartDate = startDate,
            EndDate = endDate,
            Status = status,
            ServiceLevel = serviceLevel
        };
    }
}
