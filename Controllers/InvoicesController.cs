using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechMoveLogisticsApplication.Data;

namespace TechMoveLogisticsApplication.Controllers;

[Authorize]
public class InvoicesController : Controller
{
    private readonly ApplicationDbContext _context;

    public InvoicesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var invoices = await _context.Invoices
            .Include(invoice => invoice.ServiceRequest)
            .ThenInclude(request => request!.Contract)
            .ThenInclude(contract => contract!.Client)
            .OrderByDescending(invoice => invoice.IssuedAt)
            .ToListAsync();

        return View(invoices);
    }
}
