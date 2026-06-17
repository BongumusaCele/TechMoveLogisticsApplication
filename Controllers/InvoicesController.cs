using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechMoveLogisticsApplication.Models;
using TechMoveLogisticsApplication.Services.Api;

namespace TechMoveLogisticsApplication.Controllers;

[Authorize]
public class InvoicesController : Controller
{
    private readonly ITechMoveApiClient _apiClient;

    public InvoicesController(ITechMoveApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index()
    {
        var result = await _apiClient.GetInvoicesAsync();
        if (!result.Succeeded || result.Value is null)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Invoices could not be loaded from the API.");
            return View(Enumerable.Empty<Invoice>());
        }

        return View(result.Value);
    }
}
