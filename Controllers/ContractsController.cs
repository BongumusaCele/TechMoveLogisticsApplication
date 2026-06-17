using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechMoveLogisticsApplication.Models;
using TechMoveLogisticsApplication.Services.Api;
using TechMoveLogisticsApplication.Services.Storage;
using TechMoveLogisticsApplication.ViewModels;

namespace TechMoveLogisticsApplication.Controllers;

[Authorize]
public class ContractsController : Controller
{
    private readonly ITechMoveApiClient _apiClient;
    private readonly IFileStorageService _fileStorageService;

    public ContractsController(
        ITechMoveApiClient apiClient,
        IFileStorageService fileStorageService)
    {
        _apiClient = apiClient;
        _fileStorageService = fileStorageService;
    }

    public async Task<IActionResult> Index(ContractFilterViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            viewModel.Contracts = new List<Contract>();
            return View(viewModel);
        }

        var result = await _apiClient.GetContractsAsync(viewModel.StartDate, viewModel.EndDate, viewModel.Status);
        if (!result.Succeeded || result.Value is null)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Contracts could not be loaded from the API.");
            viewModel.Contracts = new List<Contract>();
            return View(viewModel);
        }

        viewModel.Contracts = result.Value.ToList();
        return View(viewModel);
    }

    public async Task<IActionResult> Details(int id)
    {
        var result = await _apiClient.GetContractAsync(id);
        if (!result.Succeeded || result.Value is null)
        {
            return NotFound();
        }

        return View(result.Value);
    }

    public async Task<IActionResult> Create()
    {
        return View(await BuildCreateViewModelAsync(new ContractCreateViewModel()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ContractCreateViewModel viewModel)
    {
        viewModel.ServiceLevel = (viewModel.ServiceLevel ?? string.Empty).Trim();

        var fileValidation = _fileStorageService.ValidateSignedAgreement(viewModel.SignedAgreement);
        if (!fileValidation.IsValid)
        {
            ModelState.AddModelError(nameof(viewModel.SignedAgreement), fileValidation.ErrorMessage!);
        }

        if (!ModelState.IsValid)
        {
            return View(await BuildCreateViewModelAsync(viewModel));
        }

        var result = await _apiClient.CreateContractAsync(viewModel);
        if (!result.Succeeded || result.Value is null)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "The contract could not be created through the API.");
            return View(await BuildCreateViewModelAsync(viewModel));
        }

        return RedirectToAction(nameof(Details), new { id = result.Value.ContractId });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var result = await _apiClient.GetContractAsync(id);
        if (!result.Succeeded || result.Value is null)
        {
            return NotFound();
        }

        var contract = result.Value;
        var viewModel = new ContractEditViewModel
        {
            ContractId = contract.ContractId,
            ClientId = contract.ClientId,
            ContractType = contract.ContractType,
            StartDate = contract.StartDate,
            EndDate = contract.EndDate,
            Status = contract.Status,
            ServiceLevel = contract.ServiceLevel,
            ExistingAgreementFileName = contract.SignedAgreementFileName
        };

        if (contract is InternationalContract internationalContract)
        {
            viewModel.CurrencyCode = internationalContract.CurrencyCode;
            viewModel.ExchangeRule = internationalContract.ExchangeRule;
        }

        if (contract is PremiumContract premiumContract)
        {
            viewModel.PriorityLevel = premiumContract.PriorityLevel;
        }

        return View(await BuildEditViewModelAsync(viewModel));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ContractEditViewModel viewModel)
    {
        if (id != viewModel.ContractId)
        {
            return NotFound();
        }

        viewModel.ServiceLevel = (viewModel.ServiceLevel ?? string.Empty).Trim();

        if (viewModel.SignedAgreement is { Length: > 0 })
        {
            var fileValidation = _fileStorageService.ValidateSignedAgreement(viewModel.SignedAgreement);
            if (!fileValidation.IsValid)
            {
                ModelState.AddModelError(nameof(viewModel.SignedAgreement), fileValidation.ErrorMessage!);
            }
        }

        if (!ModelState.IsValid)
        {
            return View(await BuildEditViewModelAsync(viewModel));
        }

        var result = await _apiClient.UpdateContractAsync(id, viewModel);
        if (!result.Succeeded || result.Value is null)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "The contract could not be updated through the API.");
            return View(await BuildEditViewModelAsync(viewModel));
        }

        return RedirectToAction(nameof(Details), new { id = result.Value.ContractId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(int id, ContractStatus status)
    {
        if (!Enum.IsDefined(status))
        {
            TempData["StatusError"] = "Select a valid contract status.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var result = await _apiClient.UpdateContractStatusAsync(id, status);
        if (!result.Succeeded)
        {
            TempData["StatusError"] = result.ErrorMessage ?? "The contract status could not be updated through the API.";
            return RedirectToAction(nameof(Details), new { id });
        }

        TempData["StatusMessage"] = "Contract status updated successfully.";
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> DownloadAgreement(int id)
    {
        var result = await _apiClient.DownloadAgreementAsync(id);
        if (!result.Succeeded || result.Value is null)
        {
            return NotFound();
        }

        return File(result.Value.Content, result.Value.ContentType, result.Value.FileName);
    }

    private async Task<ContractCreateViewModel> BuildCreateViewModelAsync(ContractCreateViewModel viewModel)
    {
        viewModel.ClientOptions = await BuildClientOptionsAsync();
        return viewModel;
    }

    private async Task<ContractEditViewModel> BuildEditViewModelAsync(ContractEditViewModel viewModel)
    {
        viewModel.ClientOptions = await BuildClientOptionsAsync();
        return viewModel;
    }

    private async Task<IEnumerable<SelectListItem>> BuildClientOptionsAsync()
    {
        var result = await _apiClient.GetClientsAsync();
        if (!result.Succeeded || result.Value is null)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Clients could not be loaded from the API.");
            return Enumerable.Empty<SelectListItem>();
        }

        return result.Value.Select(client => new SelectListItem(client.Name, client.ClientId.ToString())).ToList();
    }
}
