using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TechMoveLogisticsApplication.Data;
using TechMoveLogisticsApplication.Models;
using TechMoveLogisticsApplication.ViewModels;

namespace TechMoveLogisticsApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var viewModel = new DashboardViewModel
            {
                ClientCount = await _context.Clients.CountAsync(),
                ActiveContractCount = await _context.Contracts.CountAsync(contract => contract.Status == ContractStatus.Active),
                ServiceRequestCount = await _context.ServiceRequests.CountAsync(),
                InvoiceCount = await _context.Invoices.CountAsync(),
                RecentContracts = await _context.Contracts
                    .Include(contract => contract.Client)
                    .OrderByDescending(contract => contract.CreatedAt)
                    .Take(5)
                    .ToListAsync(),
                RecentRequests = await _context.ServiceRequests
                    .Include(request => request.Contract)
                    .ThenInclude(contract => contract!.Client)
                    .OrderByDescending(request => request.CreatedAt)
                    .Take(5)
                    .ToListAsync()
            };

            return View(viewModel);
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
}
