using System.ComponentModel.DataAnnotations;

namespace TechMoveLogisticsApplication.Models;

public class AuditLog
{
    public int AuditLogId { get; set; }

    [Required, StringLength(80)]
    public string EventType { get; set; } = string.Empty;

    [Required, StringLength(600)]
    public string Message { get; set; } = string.Empty;

    public int? ContractId { get; set; }

    public int? ServiceRequestId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
