using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TechMoveLogisticsApplication.ViewModels;

public class ServiceRequestCreateViewModel
{
    [Required(ErrorMessage = "Select an active contract.")]
    [Display(Name = "Contract")]
    public int? ContractId { get; set; }

    [Required(ErrorMessage = "Enter the request type.")]
    [Display(Name = "Request Type")]
    [StringLength(100, ErrorMessage = "Request type cannot exceed 100 characters.")]
    public string RequestType { get; set; } = string.Empty;

    [Required(ErrorMessage = "Enter a request description.")]
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Enter the requested amount.")]
    [Range(0.01, 1_000_000, ErrorMessage = "Amount must be between 0.01 and 1,000,000 USD.")]
    [Display(Name = "Amount (USD)")]
    public decimal RequestedAmountUsd { get; set; }

    public IEnumerable<SelectListItem> ContractOptions { get; set; } = Enumerable.Empty<SelectListItem>();
}
