using TechMoveLogisticsApplication.Models;

namespace TechMoveLogisticsApplication.Services;

public interface IInvoiceService
{
    Invoice CreateInvoice(ServiceRequest request);
}
