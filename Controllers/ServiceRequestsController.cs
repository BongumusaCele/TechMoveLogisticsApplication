using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TechMoveLogisticsApplication.Data;
using TechMoveLogisticsApplication.Models;
using TechMoveLogisticsApplication.Services;
using TechMoveLogisticsApplication.Services.Currency;
using TechMoveLogisticsApplication.Services.Strategies;
using TechMoveLogisticsApplication.ViewModels;

namespace TechMoveLogisticsApplication.Controllers;

public class ServiceRequestsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrencyConversionService _currencyConversionService;
    private readonly ValidationContext _validationContext;
    private readonly IInvoiceService _invoiceService;

    public ServiceRequestsController(
        ApplicationDbContext context,
        ICurrencyConversionService currencyConversionService,
        ValidationContext validationContext,
        IInvoiceService invoiceService)
    {
        _context = context;
        _currencyConversionService = currencyConversionService;
        _validationContext = validationContext;
        _invoiceService = invoiceService;
    }

    public async Task<IActionResult> Index()
    {
        var requests = await _context.ServiceRequests
            .Include(request => request.Contract)
            .ThenInclude(contract => contract!.Client)
            .OrderByDescending(request => request.CreatedAt)
            .ToListAsync();

        return View(requests);
    }

    public async Task<IActionResult> Create()
    {
        return View(await BuildCreateViewModelAsync(new ServiceRequestCreateViewModel()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ServiceRequestCreateViewModel viewModel)
    {
        var contract = await _context.Contracts
            .Include(item => item.Client)
            .FirstOrDefaultAsync(item => item.ContractId == viewModel.ContractId);

        if (contract is null)
        {
            ModelState.AddModelError(nameof(viewModel.ContractId), "Select a valid contract.");
        }

        if (!ModelState.IsValid || contract is null)
        {
            return View(await BuildCreateViewModelAsync(viewModel));
        }

        var rate = await _currencyConversionService.GetUsdToZarRateAsync();
        var request = new ServiceRequest
        {
            ContractId = viewModel.ContractId,
            RequestType = viewModel.RequestType,
            Description = viewModel.Description,
            RequestedAmountUsd = viewModel.RequestedAmountUsd,
            CurrencyCode = "USD",
            ExchangeRate = rate,
            Cost = _currencyConversionService.ConvertUsdToZar(viewModel.RequestedAmountUsd, rate),
            Status = ServiceRequestStatus.Approved
        };

        var validation = _validationContext.ValidateRequest(contract, request);
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            return View(await BuildCreateViewModelAsync(viewModel));
        }

        _context.ServiceRequests.Add(request);
        await _context.SaveChangesAsync();

        _context.Invoices.Add(_invoiceService.CreateInvoice(request));
        _context.AuditLogs.Add(new AuditLog
        {
            EventType = "Service Request Approved",
            ContractId = request.ContractId,
            ServiceRequestId = request.ServiceRequestId,
            Message = $"Service request {request.ServiceRequestId} approved and converted at USD/ZAR rate {rate:N4}."
        });
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private async Task<ServiceRequestCreateViewModel> BuildCreateViewModelAsync(ServiceRequestCreateViewModel viewModel)
    {
        viewModel.ContractOptions = await _context.Contracts
            .Include(contract => contract.Client)
            .Where(contract => contract.Status == ContractStatus.Active)
            .OrderBy(contract => contract.Client!.Name)
            .Select(contract => new SelectListItem(
                $"{contract.Client!.Name} - {contract.ServiceLevel} ({contract.ContractType})",
                contract.ContractId.ToString()))
            .ToListAsync();

        return viewModel;
    }
}
