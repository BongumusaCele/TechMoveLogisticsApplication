using System.ComponentModel.DataAnnotations;

namespace TechMoveLogisticsApplication.Models;

public class ServiceRequest
{
    public int ServiceRequestId { get; set; }

    [Required]
    public int ContractId { get; set; }

    public Contract? Contract { get; set; }

    [Required, StringLength(100)]
    public string RequestType { get; set; } = string.Empty;

    [Required, StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required, Range(0.01, 1_000_000), Display(Name = "Requested Amount (USD)")]
    public decimal RequestedAmountUsd { get; set; }

    [Required, StringLength(3, MinimumLength = 3), Display(Name = "Currency")]
    public string CurrencyCode { get; set; } = "USD";

    [Display(Name = "Exchange Rate")]
    public decimal ExchangeRate { get; set; }

    [Display(Name = "Local Cost (ZAR)")]
    public decimal Cost { get; set; }

    public ServiceRequestStatus Status { get; set; } = ServiceRequestStatus.Submitted;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Invoice? Invoice { get; set; }
}
