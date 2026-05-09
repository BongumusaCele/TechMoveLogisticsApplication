using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechMoveLogisticsApplication.Data;
using TechMoveLogisticsApplication.Models;

namespace TechMoveLogisticsApplication.Controllers;

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
        if (!ModelState.IsValid)
        {
            return View(client);
        }

        _context.Clients.Add(client);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}
