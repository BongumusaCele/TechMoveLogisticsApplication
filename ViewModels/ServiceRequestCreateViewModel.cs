using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TechMoveLogisticsApplication.ViewModels;

public class ServiceRequestCreateViewModel
{
    [Required, Display(Name = "Contract")]
    public int ContractId { get; set; }

    [Required, Display(Name = "Request Type"), StringLength(100)]
    public string RequestType { get; set; } = string.Empty;

    [Required, StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required, Range(0.01, 1_000_000), Display(Name = "Amount (USD)")]
    public decimal RequestedAmountUsd { get; set; }

    public IEnumerable<SelectListItem> ContractOptions { get; set; } = Enumerable.Empty<SelectListItem>();
}
