using TechMoveLogisticsApplication.Data;
using TechMoveLogisticsApplication.Models;

namespace TechMoveLogisticsApplication.Services.Observers;

public class SlaMonitoringService : IContractObserver
{
    private readonly ApplicationDbContext _context;

    public SlaMonitoringService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task UpdateAsync(ContractEvent contractEvent)
    {
        if (contractEvent.Status == ContractStatus.OnHold)
        {
            _context.AuditLogs.Add(new AuditLog
            {
                EventType = "SLA Monitoring",
                ContractId = contractEvent.ContractId,
                Message = $"Contract {contractEvent.ContractId} is on hold; dependent service requests must be blocked."
            });

            await _context.SaveChangesAsync();
        }
    }
}
