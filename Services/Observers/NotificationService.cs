using TechMoveLogisticsApplication.Data;
using TechMoveLogisticsApplication.Models;

namespace TechMoveLogisticsApplication.Services.Observers;

public class NotificationService : IContractObserver
{
    private readonly ApplicationDbContext _context;

    public NotificationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task UpdateAsync(ContractEvent contractEvent)
    {
        if (contractEvent.Status is ContractStatus.Expired or ContractStatus.OnHold)
        {
            _context.AuditLogs.Add(new AuditLog
            {
                EventType = "Notification",
                ContractId = contractEvent.ContractId,
                Message = $"Notification queued for contract {contractEvent.ContractId}: status changed to {contractEvent.Status}."
            });

            await _context.SaveChangesAsync();
        }
    }
}
