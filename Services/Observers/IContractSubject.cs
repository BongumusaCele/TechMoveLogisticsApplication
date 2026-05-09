namespace TechMoveLogisticsApplication.Services.Observers;

public interface IContractSubject
{
    void Attach(IContractObserver observer);
    void Detach(IContractObserver observer);
    Task NotifyAsync(ContractEvent contractEvent);
}
