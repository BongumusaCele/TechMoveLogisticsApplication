using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechMoveLogisticsApplication.Models;
using TechMoveLogisticsApplication.Services.Api;
using TechMoveLogisticsApplication.ViewModels;

namespace TechMoveLogisticsApplication.Controllers;

public class HomeController : Controller
{
    private readonly ITechMoveApiClient _apiClient;

    public HomeController(ITechMoveApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index()
    {
        var result = await _apiClient.GetDashboardAsync();
        if (!result.Succeeded || result.Value is null)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Dashboard data could not be loaded from the API.");
            return View(new DashboardViewModel());
        }

        return View(result.Value);
    }

    [Authorize]
    public IActionResult Privacy()
    {
        return View();
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
