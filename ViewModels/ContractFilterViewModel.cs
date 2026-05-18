using System.ComponentModel.DataAnnotations;
using TechMoveLogisticsApplication.Models;

namespace TechMoveLogisticsApplication.ViewModels;

public class ContractFilterViewModel : IValidatableObject
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public ContractStatus? Status { get; set; }
    public List<Contract> Contracts { get; set; } = new();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Status.HasValue && !Enum.IsDefined(Status.Value))
        {
            yield return new ValidationResult(
                "Select a valid contract status.",
                [nameof(Status)]);
        }

        if (StartDate.HasValue && EndDate.HasValue && StartDate.Value.Date > EndDate.Value.Date)
        {
            yield return new ValidationResult(
                "The filter end date must be on or after the start date.",
                [nameof(EndDate)]);
        }
    }
}
