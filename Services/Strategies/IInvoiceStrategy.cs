using TechMoveLogisticsApplication.Models;

namespace TechMoveLogisticsApplication.Services.Strategies;

public interface IInvoiceStrategy
{
    bool CanHandle(ServiceRequest request);
    Invoice CreateInvoice(ServiceRequest request);
}
