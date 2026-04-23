using Lab8_PhamVanTung_2324801030079.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Lab8_PhamVanTung_2324801030079.Areas.Identity.Pages.Account;

[Authorize]
public class LogoutModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<LogoutModel> _logger;

    public LogoutModel(
        SignInManager<ApplicationUser> signInManager,
        ILogger<LogoutModel> logger)
    {
        _signInManager = signInManager;
        _logger = logger;
    }

    public IActionResult OnGet()
    {
        return RedirectToAction("Index", "Home", new { area = string.Empty });
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        HttpContext.Session.Clear();
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User logged out.");

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction("Index", "Home", new { area = string.Empty });
    }
}
