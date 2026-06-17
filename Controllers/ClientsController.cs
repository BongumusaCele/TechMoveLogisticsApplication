using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechMoveLogisticsApplication.Models;
using TechMoveLogisticsApplication.Services.Api;

namespace TechMoveLogisticsApplication.Controllers;

[Authorize]
public class ClientsController : Controller
{
    private readonly ITechMoveApiClient _apiClient;

    public ClientsController(ITechMoveApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index()
    {
        var result = await _apiClient.GetClientListAsync();
        if (!result.Succeeded || result.Value is null)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Clients could not be loaded from the API.");
            return View(Enumerable.Empty<Client>());
        }

        return View(result.Value);
    }

    public async Task<IActionResult> Details(int id)
    {
        var result = await _apiClient.GetClientAsync(id);
        return result.Succeeded && result.Value is not null ? View(result.Value) : NotFound();
    }

    public IActionResult Create()
    {
        return View(new Client());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Client client)
    {
        client.Name = (client.Name ?? string.Empty).Trim();
        client.ContactDetails = (client.ContactDetails ?? string.Empty).Trim();
        client.Region = (client.Region ?? string.Empty).Trim();

        if (!ModelState.IsValid)
        {
            return View(client);
        }

        var result = await _apiClient.CreateClientAsync(client);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "The client could not be created through the API.");
            return View(client);
        }

        return RedirectToAction(nameof(Index));
    }
}
