using System.ComponentModel.DataAnnotations;

namespace TechMoveLogisticsApplication.Models;

public class Invoice
{
    public int InvoiceId { get; set; }

    public int ServiceRequestId { get; set; }

    public ServiceRequest? ServiceRequest { get; set; }

    [Display(Name = "Amount (ZAR)")]
    public decimal AmountZar { get; set; }

    public InvoiceStatus Status { get; set; } = InvoiceStatus.Issued;

    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
}
