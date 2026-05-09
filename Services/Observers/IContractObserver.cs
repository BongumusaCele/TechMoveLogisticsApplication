namespace TechMoveLogisticsApplication.Services.Observers;

public interface IContractObserver
{
    Task UpdateAsync(ContractEvent contractEvent);
}
