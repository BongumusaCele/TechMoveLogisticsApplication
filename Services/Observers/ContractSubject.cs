namespace TechMoveLogisticsApplication.Services.Observers;

public class ContractSubject : IContractSubject
{
    private readonly List<IContractObserver> _observers = new();

    public void Attach(IContractObserver observer)
    {
        if (!_observers.Contains(observer))
        {
            _observers.Add(observer);
        }
    }

    public void Detach(IContractObserver observer)
    {
        _observers.Remove(observer);
    }

    public async Task NotifyAsync(ContractEvent contractEvent)
    {
        foreach (var observer in _observers)
        {
            await observer.UpdateAsync(contractEvent);
        }
    }
}
