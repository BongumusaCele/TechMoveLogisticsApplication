using TechMoveLogisticsApplication.Models;

namespace TechMoveLogisticsApplication.Services.Strategies;

public class LocalInvoiceStrategy : IInvoiceStrategy
{
    public bool CanHandle(ServiceRequest request)
    {
        return string.Equals(request.CurrencyCode, "ZAR", StringComparison.OrdinalIgnoreCase);
    }

    public Invoice CreateInvoice(ServiceRequest request)
    {
        return new Invoice
        {
            ServiceRequestId = request.ServiceRequestId,
            AmountZar = request.Cost,
            Status = InvoiceStatus.Issued
        };
    }
}
