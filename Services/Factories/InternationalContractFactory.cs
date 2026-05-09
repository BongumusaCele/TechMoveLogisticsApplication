using TechMoveLogisticsApplication.Models;

namespace TechMoveLogisticsApplication.Services.Factories;

public class InternationalContractFactory : IContractFactory
{
    public ContractType ContractType => ContractType.International;

    public Contract CreateContract(int clientId, DateTime startDate, DateTime endDate, ContractStatus status, string serviceLevel)
    {
        return new InternationalContract
        {
            ClientId = clientId,
            StartDate = startDate,
            EndDate = endDate,
            Status = status,
            ServiceLevel = serviceLevel,
            CurrencyCode = "USD",
            ExchangeRule = "Use external exchange API and store local ZAR cost"
        };
    }
}
