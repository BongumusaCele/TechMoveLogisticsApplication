using TechMoveLogisticsApplication.Models;

namespace TechMoveLogisticsApplication.ViewModels;

public class ContractFilterViewModel
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public ContractStatus? Status { get; set; }
    public List<Contract> Contracts { get; set; } = new();
}
