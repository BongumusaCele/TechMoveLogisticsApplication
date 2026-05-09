using TechMoveLogisticsApplication.Models;
using TechMoveLogisticsApplication.Services.Strategies;

namespace TechMoveLogisticsApplication.Services;

public class InvoiceService : IInvoiceService
{
    private readonly IEnumerable<IInvoiceStrategy> _strategies;

    public InvoiceService(IEnumerable<IInvoiceStrategy> strategies)
    {
        _strategies = strategies;
    }

    public Invoice CreateInvoice(ServiceRequest request)
    {
        var strategy = _strategies.FirstOrDefault(item => item.CanHandle(request))
            ?? throw new InvalidOperationException("No invoice strategy could handle the service request.");

        return strategy.CreateInvoice(request);
    }
}
