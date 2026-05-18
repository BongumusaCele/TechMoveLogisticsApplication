using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TechMoveLogisticsApplication.Data;
using TechMoveLogisticsApplication.Models;
using TechMoveLogisticsApplication.Services.Factories;
using TechMoveLogisticsApplication.Services.Observers;
using TechMoveLogisticsApplication.Services.Storage;
using TechMoveLogisticsApplication.ViewModels;

namespace TechMoveLogisticsApplication.Controllers;

[Authorize]
public class ContractsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IContractFactoryResolver _factoryResolver;
    private readonly IFileStorageService _fileStorageService;
    private readonly IContractSubject _contractSubject;

    public ContractsController(
        ApplicationDbContext context,
        IContractFactoryResolver factoryResolver,
        IFileStorageService fileStorageService,
        IContractSubject contractSubject)
    {
        _context = context;
        _factoryResolver = factoryResolver;
        _fileStorageService = fileStorageService;
        _contractSubject = contractSubject;
    }

    public async Task<IActionResult> Index(ContractFilterViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            viewModel.Contracts = new List<Contract>();
            return View(viewModel);
        }

        var query = _context.Contracts.Include(contract => contract.Client).AsQueryable();

        if (viewModel.StartDate.HasValue)
        {
            query = query.Where(contract => contract.StartDate >= viewModel.StartDate.Value);
        }

        if (viewModel.EndDate.HasValue)
        {
            query = query.Where(contract => contract.EndDate <= viewModel.EndDate.Value);
        }

        if (viewModel.Status.HasValue)
        {
            query = query.Where(contract => contract.Status == viewModel.Status.Value);
        }

        viewModel.Contracts = await query.OrderByDescending(contract => contract.CreatedAt).ToListAsync();

        return View(viewModel);
    }

    public async Task<IActionResult> Details(int id)
    {
        var contract = await _context.Contracts
            .Include(item => item.Client)
            .Include(item => item.ServiceRequests)
            .FirstOrDefaultAsync(item => item.ContractId == id);

        if (contract is null)
        {
            return NotFound();
        }

        return View(contract);
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

        if (viewModel.ClientId.HasValue && !await _context.Clients.AnyAsync(client => client.ClientId == viewModel.ClientId.Value))
        {
            ModelState.AddModelError(nameof(viewModel.ClientId), "Select an existing client.");
        }

        if (!ModelState.IsValid)
        {
            return View(await BuildCreateViewModelAsync(viewModel));
        }

        var factory = _factoryResolver.Resolve(viewModel.ContractType);
        var contract = factory.CreateContract(
            viewModel.ClientId!.Value,
            viewModel.StartDate,
            viewModel.EndDate,
            viewModel.Status,
            viewModel.ServiceLevel);

        if (!contract.Validate())
        {
            ModelState.AddModelError(string.Empty, "The contract failed business validation.");
            return View(await BuildCreateViewModelAsync(viewModel));
        }

        try
        {
            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync();

            try
            {
                contract.SignedAgreementFileName = await _fileStorageService.SaveContractAgreementAsync(viewModel.SignedAgreement, contract.ContractId);
            }
            catch (Exception exception) when (exception is InvalidOperationException or IOException)
            {
                _context.Contracts.Remove(contract);
                await _context.SaveChangesAsync();
                ModelState.AddModelError(nameof(viewModel.SignedAgreement), "The agreement file could not be saved. Upload a valid PDF and try again.");
                return View(await BuildCreateViewModelAsync(viewModel));
            }

            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "The contract could not be saved. Check the details and try again.");
            return View(await BuildCreateViewModelAsync(viewModel));
        }

        await _contractSubject.NotifyAsync(new ContractEvent
        {
            ContractId = contract.ContractId,
            Status = contract.Status,
            EventType = "Contract Created"
        });

        return RedirectToAction(nameof(Details), new { id = contract.ContractId });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var contract = await _context.Contracts
            .Include(item => item.Client)
            .FirstOrDefaultAsync(item => item.ContractId == id);

        if (contract is null)
        {
            return NotFound();
        }

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

        var contract = await _context.Contracts.FindAsync(id);
        if (contract is null)
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

        if (viewModel.ClientId.HasValue && !await _context.Clients.AnyAsync(client => client.ClientId == viewModel.ClientId.Value))
        {
            ModelState.AddModelError(nameof(viewModel.ClientId), "Select an existing client.");
        }

        if (!ModelState.IsValid)
        {
            viewModel.ContractType = contract.ContractType;
            viewModel.ExistingAgreementFileName = contract.SignedAgreementFileName;
            return View(await BuildEditViewModelAsync(viewModel));
        }

        var previousStatus = contract.Status;
        contract.ClientId = viewModel.ClientId!.Value;
        contract.StartDate = viewModel.StartDate;
        contract.EndDate = viewModel.EndDate;
        contract.Status = viewModel.Status;
        contract.ServiceLevel = viewModel.ServiceLevel;

        if (contract is InternationalContract internationalContract)
        {
            internationalContract.CurrencyCode = (viewModel.CurrencyCode ?? "USD").ToUpperInvariant();
            internationalContract.ExchangeRule = viewModel.ExchangeRule ?? "Use external exchange API and store local ZAR cost";
        }

        if (contract is PremiumContract premiumContract)
        {
            premiumContract.PriorityLevel = viewModel.PriorityLevel ?? 1;
        }

        if (!contract.Validate())
        {
            ModelState.AddModelError(string.Empty, "The contract failed business validation. Check the date range and contract-specific fields.");
            viewModel.ContractType = contract.ContractType;
            viewModel.ExistingAgreementFileName = contract.SignedAgreementFileName;
            return View(await BuildEditViewModelAsync(viewModel));
        }

        if (viewModel.SignedAgreement is { Length: > 0 })
        {
            try
            {
                contract.SignedAgreementFileName = await _fileStorageService.SaveContractAgreementAsync(viewModel.SignedAgreement, contract.ContractId);
            }
            catch (Exception exception) when (exception is InvalidOperationException or IOException)
            {
                ModelState.AddModelError(nameof(viewModel.SignedAgreement), "The agreement file could not be saved. Upload a valid PDF and try again.");
                viewModel.ContractType = contract.ContractType;
                viewModel.ExistingAgreementFileName = contract.SignedAgreementFileName;
                return View(await BuildEditViewModelAsync(viewModel));
            }
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "The contract changes could not be saved. Check the details and try again.");
            viewModel.ContractType = contract.ContractType;
            viewModel.ExistingAgreementFileName = contract.SignedAgreementFileName;
            return View(await BuildEditViewModelAsync(viewModel));
        }

        await _contractSubject.NotifyAsync(new ContractEvent
        {
            ContractId = contract.ContractId,
            Status = contract.Status,
            EventType = previousStatus == contract.Status ? "Contract Updated" : "Contract Status Changed"
        });

        return RedirectToAction(nameof(Details), new { id = contract.ContractId });
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

        var contract = await _context.Contracts.FindAsync(id);
        if (contract is null)
        {
            return NotFound();
        }

        contract.Status = status;
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            TempData["StatusError"] = "The contract status could not be updated. Try again.";
            return RedirectToAction(nameof(Details), new { id });
        }

        await _contractSubject.NotifyAsync(new ContractEvent
        {
            ContractId = contract.ContractId,
            Status = contract.Status,
            EventType = "Contract Status Changed"
        });

        TempData["StatusMessage"] = "Contract status updated successfully.";
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> DownloadAgreement(int id)
    {
        var contract = await _context.Contracts.FindAsync(id);
        if (contract?.SignedAgreementFileName is null)
        {
            return NotFound();
        }

        string path;
        try
        {
            path = _fileStorageService.GetSignedAgreementPath(contract.SignedAgreementFileName);
        }
        catch (InvalidOperationException)
        {
            return BadRequest("The agreement file reference is invalid.");
        }

        if (!System.IO.File.Exists(path))
        {
            return NotFound();
        }

        return PhysicalFile(path, "application/pdf", contract.SignedAgreementFileName);
    }

    private async Task<ContractCreateViewModel> BuildCreateViewModelAsync(ContractCreateViewModel viewModel)
    {
        viewModel.ClientOptions = await _context.Clients
            .OrderBy(client => client.Name)
            .Select(client => new SelectListItem(client.Name, client.ClientId.ToString()))
            .ToListAsync();

        return viewModel;
    }

    private async Task<ContractEditViewModel> BuildEditViewModelAsync(ContractEditViewModel viewModel)
    {
        viewModel.ClientOptions = await _context.Clients
            .OrderBy(client => client.Name)
            .Select(client => new SelectListItem(client.Name, client.ClientId.ToString()))
            .ToListAsync();

        return viewModel;
    }
}
