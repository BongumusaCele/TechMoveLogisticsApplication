using System.ComponentModel.DataAnnotations;

namespace TechMoveLogisticsApplication.Models;

public class Client
{
    public int ClientId { get; set; }

    [Required(ErrorMessage = "Enter the client name.")]
    [StringLength(120, ErrorMessage = "Client name cannot exceed 120 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Enter contact details for this client.")]
    [Display(Name = "Contact Details")]
    [StringLength(200, ErrorMessage = "Contact details cannot exceed 200 characters.")]
    public string ContactDetails { get; set; } = string.Empty;

    [Required(ErrorMessage = "Enter the client region.")]
    [StringLength(80, ErrorMessage = "Region cannot exceed 80 characters.")]
    public string Region { get; set; } = string.Empty;

    public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
}
