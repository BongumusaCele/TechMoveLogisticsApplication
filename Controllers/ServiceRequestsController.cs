using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TechMoveLogisticsApplication.Data;
using TechMoveLogisticsApplication.Models;
using TechMoveLogisticsApplication.Services.Workflow;
using TechMoveLogisticsApplication.ViewModels;

namespace TechMoveLogisticsApplication.Controllers;

public class ServiceRequestsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IServiceRequestWorkflowService _workflowService;

    public ServiceRequestsController(
        ApplicationDbContext context,
        IServiceRequestWorkflowService workflowService)
    {
        _context = context;
        _workflowService = workflowService;
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
        if (!ModelState.IsValid)
        {
            return View(await BuildCreateViewModelAsync(viewModel));
        }

        var result = await _workflowService.CreateApprovedRequestAsync(viewModel);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.FieldName ?? string.Empty, error.Message);
            }

            return View(await BuildCreateViewModelAsync(viewModel));
        }

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
