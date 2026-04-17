using IEmailSender = ASCwed.Services.IEmailSender;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace ASCwed.Areas.Identity.Pages.Account
{
    [Authorize]
    public class InitiateResetPasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;

        public InitiateResetPasswordModel(UserManager<IdentityUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        public IActionResult OnGet()
        {
            return Redirect("/Accounts/Account/Profile");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || string.IsNullOrWhiteSpace(user.Email))
            {
                return RedirectToPage("./Login");
            }

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ResetPassword",
                pageHandler: null,
                values: new { area = "Identity", code, email = user.Email },
                protocol: Request.Scheme) ?? string.Empty;

            await _emailSender.SendEmailAsync(
                user.Email,
                "Reset your password",
                $"Please reset your password by <a href='{callbackUrl}'>clicking here</a>.");

            return RedirectToPage("./ResetPasswordEmailConfirmation", new { email = user.Email });
        }
    }
}
