using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechMoveLogisticsApplication.Services.Api;
using TechMoveLogisticsApplication.ViewModels;

namespace TechMoveLogisticsApplication.Controllers;

public class AuthController : Controller
{
    private readonly ITechMoveApiClient _apiClient;

    public AuthController(ITechMoveApiClient apiClient)
    {
        _apiClient = apiClient;
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

        viewModel.Email = (viewModel.Email ?? string.Empty).Trim();
        var result = await _apiClient.LoginAsync(viewModel);
        if (!result.Succeeded || result.Value is null)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Invalid email address or password.");
            return View(viewModel);
        }

        await SignInAsync(result.Value.UserId, result.Value.FullName, result.Value.Email, result.Value.Role, viewModel.RememberMe);

        if (Url.IsLocalUrl(viewModel.ReturnUrl))
        {
            return Redirect(viewModel.ReturnUrl);
        }

        return RedirectToRoleHome(result.Value.Role);
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

        var result = await _apiClient.RegisterAsync(viewModel);
        if (!result.Succeeded || result.Value is null)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Your account could not be created. Check the details and try again.");
            return View(viewModel);
        }

        await SignInAsync(result.Value.UserId, result.Value.FullName, result.Value.Email, result.Value.Role, rememberMe: false);
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

    private async Task SignInAsync(int userId, string fullName, string email, string role, bool rememberMe)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, fullName),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, role)
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
