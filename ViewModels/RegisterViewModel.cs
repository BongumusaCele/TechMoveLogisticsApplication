using System.ComponentModel.DataAnnotations;

namespace TechMoveLogisticsApplication.ViewModels;

public class RegisterViewModel : IValidatableObject
{
    [Required(ErrorMessage = "Enter your full name.")]
    [Display(Name = "Full name")]
    [StringLength(80, ErrorMessage = "Full name cannot exceed 80 characters.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Enter your email address.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [StringLength(160, ErrorMessage = "Email address cannot exceed 160 characters.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Enter a password.")]
    [DataType(DataType.Password)]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirm your password.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Password))
        {
            yield break;
        }

        if (!Password.Any(char.IsUpper)
            || !Password.Any(char.IsLower)
            || !Password.Any(char.IsDigit)
            || !Password.Any(character => !char.IsLetterOrDigit(character)))
        {
            yield return new ValidationResult(
                "Password must include uppercase, lowercase, number, and special characters.",
                [nameof(Password)]);
        }
    }
}
