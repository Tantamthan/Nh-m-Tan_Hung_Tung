using System.ComponentModel.DataAnnotations;
using Lab8_PhamVanTung_2324801030079.Constants;
using Lab8_PhamVanTung_2324801030079.Extensions;
using Lab8_PhamVanTung_2324801030079.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Lab8_PhamVanTung_2324801030079.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ILogger<LoginModel> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string ReturnUrl { get; set; } = string.Empty;

    public async Task OnGetAsync(string? returnUrl = null)
    {
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        HttpContext.Session.Clear();

        ReturnUrl = returnUrl ?? Url.Action("Dashboard", "Dashboard", new { area = "ServiceRequests" }) ?? "/";
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Action("Dashboard", "Dashboard", new { area = "ServiceRequests" }) ?? "/";

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _userManager.FindByEmailAsync(Input.Email);

        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
            return Page();
        }

        var signInResult = await _signInManager.PasswordSignInAsync(
            user,
            Input.Password,
            Input.RememberMe,
            lockoutOnFailure: false);

        if (signInResult.Succeeded)
        {
            var principal = await _signInManager.CreateUserPrincipalAsync(user);
            HttpContext.Session.SetObject(SessionConstants.CurrentUser, principal.ToCurrentUser());

            _logger.LogInformation("User {Email} logged in successfully.", Input.Email);

            if (Url.IsLocalUrl(ReturnUrl))
            {
                return LocalRedirect(ReturnUrl);
            }

            return RedirectToAction("Dashboard", "Dashboard", new { area = "ServiceRequests" });
        }

        if (signInResult.IsLockedOut)
        {
            _logger.LogWarning("Locked out login attempt detected for {Email}.", Input.Email);
            ModelState.AddModelError(string.Empty, "Tài khoản đang bị khóa tạm thời.");
            return Page();
        }

        ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
        return Page();
    }

    public sealed class InputModel
    {
        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }
}
