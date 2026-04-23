using System.ComponentModel.DataAnnotations;
using System.Text;
using Lab8_PhamVanTung_2324801030079.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace Lab8_PhamVanTung_2324801030079.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class ForgotPasswordModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<ForgotPasswordModel> _logger;

    public ForgotPasswordModel(
        UserManager<ApplicationUser> userManager,
        IEmailSender emailSender,
        ILogger<ForgotPasswordModel> logger)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _userManager.FindByEmailAsync(Input.Email);

        if (user is null || string.IsNullOrWhiteSpace(user.Email))
        {
            return RedirectToPage("./ForgotPasswordConfirmation");
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
             <p>Bạn vừa yêu cầu đặt lại mật khẩu cho tài khoản <strong>{user.Email}</strong>.</p>
             <p>Nhấn vào link sau để tiếp tục: <a href="{callbackUrl}">Reset Password</a></p>
             <p>Nếu bạn không thực hiện yêu cầu này, hãy bỏ qua email.</p>
             """);

        _logger.LogInformation("Forgot password email queued for {Email}.", user.Email);
        return RedirectToPage("./ForgotPasswordConfirmation");
    }

    public sealed class InputModel
    {
        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
    }
}
