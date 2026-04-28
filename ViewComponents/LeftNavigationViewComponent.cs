using Lab8_PhamVanTung_2324801030079.Models;
using Lab8_PhamVanTung_2324801030079.Services.Navigation;
using Microsoft.AspNetCore.Mvc;

namespace Lab8_PhamVanTung_2324801030079.ViewComponents;

public sealed class LeftNavigationViewComponent : ViewComponent
{
    private readonly INavigationCacheOperations _navigationCacheOperations;

    public LeftNavigationViewComponent(INavigationCacheOperations navigationCacheOperations)
    {
        _navigationCacheOperations = navigationCacheOperations;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var roles = HttpContext.User.Claims
            .Where(claim => claim.Type.EndsWith("/role", StringComparison.OrdinalIgnoreCase)
                || string.Equals(claim.Type, System.Security.Claims.ClaimTypes.Role, StringComparison.OrdinalIgnoreCase))
            .Select(claim => claim.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var items = await _navigationCacheOperations.GetNavigationAsync();
        var filteredItems = FilterItems(items, roles);

        return View(filteredItems);
    }

    private static IReadOnlyList<NavigationItem> FilterItems(IEnumerable<NavigationItem> source, IReadOnlyCollection<string> roles)
    {
        var filtered = new List<NavigationItem>();

        foreach (var item in source)
        {
            var visibleChildren = FilterItems(item.Children, roles);

            if (item.IsVisibleTo(roles) || visibleChildren.Count > 0)
            {
                filtered.Add(item.CloneWithChildren(visibleChildren));
            }
        }

        return filtered;
    }
}
