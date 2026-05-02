using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ASCwed.Areas.Identity.Pages.Account
{
    public class ResetPasswordEmailConfirmationModel : PageModel
    {
        public string Email { get; set; } = string.Empty;

        public void OnGet(string? email = null)
        {
            Email = email ?? "your registered address";
        }
    }
}
