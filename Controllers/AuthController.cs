using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechMoveLogisticsApplication.Data;
using TechMoveLogisticsApplication.Models;
using TechMoveLogisticsApplication.ViewModels;

namespace TechMoveLogisticsApplication.Controllers;

public class AuthController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly PasswordHasher<ApplicationUser> _passwordHasher = new();

    public AuthController(ApplicationDbContext context)
    {
        _context = context;
    }

    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToRoleHome(User);
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var normalizedEmail = (viewModel.Email ?? string.Empty).Trim().ToUpperInvariant();
        ApplicationUser? user;
        try
        {
            user = await _context.ApplicationUsers
                .FirstOrDefaultAsync(item => item.Email.ToUpper() == normalizedEmail);
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Unable to sign in right now. Try again shortly.");
            return View(viewModel);
        }

        if (user is null || !IsValidPassword(user, viewModel.Password))
        {
            ModelState.AddModelError(string.Empty, "Invalid email address or password.");
            return View(viewModel);
        }

        await SignInAsync(user, viewModel.RememberMe);

        if (Url.IsLocalUrl(viewModel.ReturnUrl))
        {
            return Redirect(viewModel.ReturnUrl);
        }

        return RedirectToRoleHome(user.Role);
    }

    [AllowAnonymous]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToRoleHome(User);
        }

        return View(new RegisterViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        viewModel.FullName = (viewModel.FullName ?? string.Empty).Trim();
        viewModel.Email = (viewModel.Email ?? string.Empty).Trim();

        var normalizedEmail = viewModel.Email.ToUpperInvariant();
        var emailExists = await _context.ApplicationUsers.AnyAsync(user => user.Email.ToUpper() == normalizedEmail);
        if (emailExists)
        {
            ModelState.AddModelError(nameof(viewModel.Email), "An account with this email address already exists.");
            return View(viewModel);
        }

        var user = new ApplicationUser
        {
            FullName = viewModel.FullName,
            Email = viewModel.Email,
            Role = "User"
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, viewModel.Password);

        try
        {
            _context.ApplicationUsers.Add(user);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Your account could not be created. Check the details and try again.");
            return View(viewModel);
        }

        await SignInAsync(user, rememberMe: false);
        return RedirectToAction("Index", "Contracts");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }

    private bool IsValidPassword(ApplicationUser user, string password)
    {
        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }

    private async Task SignInAsync(ApplicationUser user, bool rememberMe)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.ApplicationUserId.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var properties = new AuthenticationProperties
        {
            IsPersistent = rememberMe,
            ExpiresUtc = rememberMe ? DateTimeOffset.UtcNow.AddDays(7) : null
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, properties);
    }

    private IActionResult RedirectToRoleHome(ClaimsPrincipal principal)
    {
        return RedirectToRoleHome(principal.IsInRole("Admin") ? "Admin" : "User");
    }

    private IActionResult RedirectToRoleHome(string role)
    {
        return string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase)
            ? RedirectToAction("Index", "Home")
            : RedirectToAction("Index", "Contracts");
    }
}
