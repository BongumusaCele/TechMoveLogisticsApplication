using TechMoveLogisticsApplication.Models;

namespace TechMoveLogisticsApplication.Services.Factories;

public interface IContractFactory
{
    ContractType ContractType { get; }
    Contract CreateContract(int clientId, DateTime startDate, DateTime endDate, ContractStatus status, string serviceLevel);
}
