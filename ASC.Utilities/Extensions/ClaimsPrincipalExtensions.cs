using ASC.Utilities.Models;
using System.Security.Claims;

namespace ASC.Utilities.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetUserId(this ClaimsPrincipal principal)
        {
            return principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }

        public static string GetUserName(this ClaimsPrincipal principal)
        {
            return principal.Identity?.Name ?? principal.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
        }

        public static string GetEmailAddress(this ClaimsPrincipal principal)
        {
            return principal.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        }

        public static List<string> GetRoles(this ClaimsPrincipal principal)
        {
            return principal.FindAll(ClaimTypes.Role)
                .Select(item => item.Value)
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public static CurrentUser ToCurrentUser(this ClaimsPrincipal principal)
        {
            if (principal.Identity?.IsAuthenticated != true)
            {
                return new CurrentUser();
            }

            return new CurrentUser
            {
                Id = principal.GetUserId(),
                UserName = principal.GetUserName(),
                Email = principal.GetEmailAddress(),
                Roles = principal.GetRoles()
            };
        }
    }
}
