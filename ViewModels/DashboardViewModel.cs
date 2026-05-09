using TechMoveLogisticsApplication.Models;

namespace TechMoveLogisticsApplication.ViewModels;

public class DashboardViewModel
{
    public int ClientCount { get; set; }
    public int ActiveContractCount { get; set; }
    public int ServiceRequestCount { get; set; }
    public int InvoiceCount { get; set; }
    public List<Contract> RecentContracts { get; set; } = new();
    public List<ServiceRequest> RecentRequests { get; set; } = new();
}
