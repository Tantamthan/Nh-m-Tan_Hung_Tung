using System.Security.Claims;
using Lab8_PhamVanTung_2324801030079.Models.Security;

namespace Lab8_PhamVanTung_2324801030079.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static CurrentUser ToCurrentUser(this ClaimsPrincipal principal)
    {
        var email = principal.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        var userName = principal.Identity?.Name ?? email;

        return new CurrentUser
        {
            UserId = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty,
            UserName = userName,
            Email = email,
            DisplayName = principal.FindFirstValue(ClaimTypes.GivenName) ?? userName,
            Roles = principal.FindAll(ClaimTypes.Role)
                .Select(claim => claim.Value)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList(),
            IsAuthenticated = principal.Identity?.IsAuthenticated ?? false
        };
    }

    public static string? GetUserIdValue(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    public static string? GetEmailValue(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimTypes.Email);
    }

    public static IReadOnlyCollection<string> GetRoleValues(this ClaimsPrincipal principal)
    {
        return principal.FindAll(ClaimTypes.Role)
            .Select(claim => claim.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
