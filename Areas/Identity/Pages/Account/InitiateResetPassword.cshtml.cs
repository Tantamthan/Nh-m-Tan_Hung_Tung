using System.Text;
using Lab8_PhamVanTung_2324801030079.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace Lab8_PhamVanTung_2324801030079.Areas.Identity.Pages.Account;

[Authorize]
public class InitiateResetPasswordModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<InitiateResetPasswordModel> _logger;

    public InitiateResetPasswordModel(
        UserManager<ApplicationUser> userManager,
        IEmailSender emailSender,
        ILogger<InitiateResetPasswordModel> logger)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _logger = logger;
    }

    public string CurrentEmail { get; private set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null || string.IsNullOrWhiteSpace(user.Email))
        {
            return Challenge();
        }

        CurrentEmail = user.Email;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null || string.IsNullOrWhiteSpace(user.Email))
        {
            return Challenge();
        }

        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        var callbackUrl = Url.Page(
            "/Account/ResetPassword",
            pageHandler: null,
            values: new { area = "Identity", code, email = user.Email },
            protocol: Request.Scheme);

        if (string.IsNullOrWhiteSpace(callbackUrl))
        {
            throw new InvalidOperationException("Unable to generate password reset callback URL.");
        }

        await _emailSender.SendEmailAsync(
            user.Email,
            "Reset your password",
            $"""
             <p>Xin chào,</p>
             <p>Bạn vừa yêu cầu reset mật khẩu từ bên trong hệ thống.</p>
             <p>Nhấn vào link sau để tiếp tục: <a href="{callbackUrl}">Reset Password</a></p>
             <p>Link này được tạo cho tài khoản <strong>{user.Email}</strong>.</p>
             """);

        _logger.LogInformation("Authenticated reset password email sent for {Email}.", user.Email);
        return RedirectToPage("./ResetPasswordEmailConfirmation");
    }
}
