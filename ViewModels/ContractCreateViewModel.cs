using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechMoveLogisticsApplication.Models;

namespace TechMoveLogisticsApplication.ViewModels;

public class ContractCreateViewModel : IValidatableObject
{
    [Required(ErrorMessage = "Select a client.")]
    [Display(Name = "Client")]
    public int? ClientId { get; set; }

    [Required(ErrorMessage = "Select a contract type.")]
    [Display(Name = "Contract Type")]
    public ContractType ContractType { get; set; }

    [Required(ErrorMessage = "Enter the contract start date.")]
    [DataType(DataType.Date)]
    [Display(Name = "Start Date")]
    public DateTime StartDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Enter the contract end date.")]
    [DataType(DataType.Date)]
    [Display(Name = "End Date")]
    public DateTime EndDate { get; set; } = DateTime.Today.AddMonths(12);

    [Required(ErrorMessage = "Select a contract status.")]
    public ContractStatus Status { get; set; } = ContractStatus.Draft;

    [Required(ErrorMessage = "Enter the service level.")]
    [Display(Name = "Service Level")]
    [StringLength(80, ErrorMessage = "Service level cannot exceed 80 characters.")]
    public string ServiceLevel { get; set; } = string.Empty;

    [Required(ErrorMessage = "Upload a signed PDF agreement.")]
    [Display(Name = "Signed Agreement PDF")]
    public IFormFile? SignedAgreement { get; set; }

    public IEnumerable<SelectListItem> ClientOptions { get; set; } = Enumerable.Empty<SelectListItem>();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!Enum.IsDefined(ContractType))
        {
            yield return new ValidationResult(
                "Select a valid contract type.",
                [nameof(ContractType)]);
        }

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
