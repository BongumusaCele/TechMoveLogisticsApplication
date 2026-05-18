using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechMoveLogisticsApplication.Models;

namespace TechMoveLogisticsApplication.ViewModels;

public class ContractEditViewModel : IValidatableObject
{
    public int ContractId { get; set; }

    [Required(ErrorMessage = "Select a client.")]
    [Display(Name = "Client")]
    public int? ClientId { get; set; }

    [Display(Name = "Contract Type")]
    public ContractType ContractType { get; set; }

    [Required(ErrorMessage = "Enter the contract start date.")]
    [DataType(DataType.Date)]
    [Display(Name = "Start Date")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "Enter the contract end date.")]
    [DataType(DataType.Date)]
    [Display(Name = "End Date")]
    public DateTime EndDate { get; set; }

    [Required(ErrorMessage = "Select a contract status.")]
    public ContractStatus Status { get; set; }

    [Required(ErrorMessage = "Enter the service level.")]
    [Display(Name = "Service Level")]
    [StringLength(80, ErrorMessage = "Service level cannot exceed 80 characters.")]
    public string ServiceLevel { get; set; } = string.Empty;

    [Display(Name = "Replace Signed Agreement PDF")]
    public IFormFile? SignedAgreement { get; set; }

    public string? ExistingAgreementFileName { get; set; }

    [Display(Name = "Currency Code")]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency code must be exactly 3 characters.")]
    public string? CurrencyCode { get; set; }

    [Display(Name = "Exchange Rule")]
    [StringLength(120, ErrorMessage = "Exchange rule cannot exceed 120 characters.")]
    public string? ExchangeRule { get; set; }

    [Range(1, 5, ErrorMessage = "Priority level must be between 1 and 5.")]
    [Display(Name = "Priority Level")]
    public int? PriorityLevel { get; set; }

    public IEnumerable<SelectListItem> ClientOptions { get; set; } = Enumerable.Empty<SelectListItem>();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!Enum.IsDefined(Status))
        {
            yield return new ValidationResult(
                "Select a valid contract status.",
                [nameof(Status)]);
        }

        if (StartDate.Date > EndDate.Date)
        {
            yield return new ValidationResult(
                "The contract end date must be on or after the start date.",
                [nameof(EndDate)]);
        }
    }
}
