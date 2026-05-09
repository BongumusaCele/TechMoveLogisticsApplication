using TechMoveLogisticsApplication.Data;
using TechMoveLogisticsApplication.Models;

namespace TechMoveLogisticsApplication.Services.Observers;

public class AuditService : IContractObserver
{
    private readonly ApplicationDbContext _context;

    public AuditService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task UpdateAsync(ContractEvent contractEvent)
    {
        _context.AuditLogs.Add(new AuditLog
        {
            EventType = contractEvent.EventType,
            ContractId = contractEvent.ContractId,
            Message = $"Contract {contractEvent.ContractId} event '{contractEvent.EventType}' recorded with status {contractEvent.Status}."
        });

        await _context.SaveChangesAsync();
    }
}
