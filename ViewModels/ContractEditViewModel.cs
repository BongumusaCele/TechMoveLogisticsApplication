using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechMoveLogisticsApplication.Models;

namespace TechMoveLogisticsApplication.ViewModels;

public class ContractEditViewModel
{
    public int ContractId { get; set; }

    [Required, Display(Name = "Client")]
    public int ClientId { get; set; }

    [Display(Name = "Contract Type")]
    public ContractType ContractType { get; set; }

    [Required, DataType(DataType.Date), Display(Name = "Start Date")]
    public DateTime StartDate { get; set; }

    [Required, DataType(DataType.Date), Display(Name = "End Date")]
    public DateTime EndDate { get; set; }

    [Required]
    public ContractStatus Status { get; set; }

    [Required, Display(Name = "Service Level"), StringLength(80)]
    public string ServiceLevel { get; set; } = string.Empty;

    [Display(Name = "Replace Signed Agreement PDF")]
    public IFormFile? SignedAgreement { get; set; }

    public string? ExistingAgreementFileName { get; set; }

    [Display(Name = "Currency Code"), StringLength(3, MinimumLength = 3)]
    public string? CurrencyCode { get; set; }

    [Display(Name = "Exchange Rule"), StringLength(120)]
    public string? ExchangeRule { get; set; }

    [Range(1, 5), Display(Name = "Priority Level")]
    public int? PriorityLevel { get; set; }

    public IEnumerable<SelectListItem> ClientOptions { get; set; } = Enumerable.Empty<SelectListItem>();
}
