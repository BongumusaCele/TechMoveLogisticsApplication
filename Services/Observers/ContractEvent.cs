using TechMoveLogisticsApplication.Models;

namespace TechMoveLogisticsApplication.Services.Observers;

public class ContractEvent
{
    public int ContractId { get; init; }
    public ContractStatus Status { get; init; }
    public string EventType { get; init; } = string.Empty;
    public DateTime EventTime { get; init; } = DateTime.UtcNow;
}
