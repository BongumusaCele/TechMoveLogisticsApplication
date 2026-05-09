using TechMoveLogisticsApplication.Models;

namespace TechMoveLogisticsApplication.Services.Factories;

public class ContractFactoryResolver : IContractFactoryResolver
{
    private readonly IEnumerable<IContractFactory> _factories;

    public ContractFactoryResolver(IEnumerable<IContractFactory> factories)
    {
        _factories = factories;
    }

    public IContractFactory Resolve(ContractType contractType)
    {
        return _factories.FirstOrDefault(factory => factory.ContractType == contractType)
            ?? throw new InvalidOperationException($"No contract factory is registered for {contractType}.");
    }
}
