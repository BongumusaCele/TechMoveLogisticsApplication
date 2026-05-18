using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechMoveLogisticsApplication.Data;
using TechMoveLogisticsApplication.Models;

namespace TechMoveLogisticsApplication.Controllers;

[Authorize]
public class ClientsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ClientsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var clients = await _context.Clients
            .Include(client => client.Contracts)
            .OrderBy(client => client.Name)
            .ToListAsync();

        return View(clients);
    }

    public async Task<IActionResult> Details(int id)
    {
        var client = await _context.Clients
            .Include(item => item.Contracts)
            .FirstOrDefaultAsync(item => item.ClientId == id);

        if (client is null)
        {
            return NotFound();
        }

        return View(client);
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

        if (await _context.Clients.AnyAsync(item => item.Name == client.Name))
        {
            ModelState.AddModelError(nameof(client.Name), "A client with this name already exists.");
        }

        if (!ModelState.IsValid)
        {
            return View(client);
        }

        try
        {
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "The client could not be saved. Check the details and try again.");
            return View(client);
        }

        return RedirectToAction(nameof(Index));
    }
}
