using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechMoveLogisticsApplication.Models;

namespace TechMoveLogisticsApplication.ViewModels;

public class ContractCreateViewModel
{
    [Required, Display(Name = "Client")]
    public int ClientId { get; set; }

    [Required, Display(Name = "Contract Type")]
    public ContractType ContractType { get; set; }

    [Required, DataType(DataType.Date), Display(Name = "Start Date")]
    public DateTime StartDate { get; set; } = DateTime.Today;

    [Required, DataType(DataType.Date), Display(Name = "End Date")]
    public DateTime EndDate { get; set; } = DateTime.Today.AddMonths(12);

    [Required]
    public ContractStatus Status { get; set; } = ContractStatus.Draft;

    [Required, Display(Name = "Service Level"), StringLength(80)]
    public string ServiceLevel { get; set; } = string.Empty;

    [Required, Display(Name = "Signed Agreement PDF")]
    public IFormFile? SignedAgreement { get; set; }

    public IEnumerable<SelectListItem> ClientOptions { get; set; } = Enumerable.Empty<SelectListItem>();
}
