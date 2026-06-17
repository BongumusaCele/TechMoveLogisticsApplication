using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechMoveLogisticsApplication.Models;
using TechMoveLogisticsApplication.Services.Api;

namespace TechMoveLogisticsApplication.Controllers;

[Authorize(Roles = "Admin")]
public class AuditController : Controller
{
    private readonly ITechMoveApiClient _apiClient;

    public AuditController(ITechMoveApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index()
    {
        var result = await _apiClient.GetAuditLogsAsync();
        if (!result.Succeeded || result.Value is null)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Audit logs could not be loaded from the API.");
            return View(Enumerable.Empty<AuditLog>());
        }

        return View(result.Value);
    }
}
