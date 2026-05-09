using TechMoveLogisticsApplication.Models;

namespace TechMoveLogisticsApplication.Services.Factories;

public interface IContractFactoryResolver
{
    IContractFactory Resolve(ContractType contractType);
}
