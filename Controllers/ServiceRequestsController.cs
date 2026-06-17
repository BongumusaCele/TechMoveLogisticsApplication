using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechMoveLogisticsApplication.Models;
using TechMoveLogisticsApplication.Services.Api;
using TechMoveLogisticsApplication.ViewModels;

namespace TechMoveLogisticsApplication.Controllers;

[Authorize]
public class ServiceRequestsController : Controller
{
    private readonly ITechMoveApiClient _apiClient;

    public ServiceRequestsController(ITechMoveApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index()
    {
        var result = await _apiClient.GetServiceRequestsAsync();
        if (!result.Succeeded || result.Value is null)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Service requests could not be loaded from the API.");
            return View(Enumerable.Empty<ServiceRequest>());
        }

        return View(result.Value);
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

        var result = await _apiClient.CreateServiceRequestAsync(viewModel);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "The service request could not be created through the API.");
            return View(await BuildCreateViewModelAsync(viewModel));
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<ServiceRequestCreateViewModel> BuildCreateViewModelAsync(ServiceRequestCreateViewModel viewModel)
    {
        var contracts = await _apiClient.GetContractsAsync(null, null, ContractStatus.Active);
        if (!contracts.Succeeded || contracts.Value is null)
        {
            ModelState.AddModelError(string.Empty, contracts.ErrorMessage ?? "Active contracts could not be loaded from the API.");
            viewModel.ContractOptions = Enumerable.Empty<SelectListItem>();
            return viewModel;
        }

        viewModel.ContractOptions = contracts.Value
            .OrderBy(contract => contract.Client?.Name)
            .Select(contract => new SelectListItem(
                $"{contract.Client!.Name} - {contract.ServiceLevel} ({contract.ContractType})",
                contract.ContractId.ToString()))
            .ToList();

        return viewModel;
    }
}
