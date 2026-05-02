using ASC.Utilities.Extensions;
using ASC.Utilities.Models;
using ASCwed.Models.Navigation;
using ASCwed.Services.Navigation;
using Microsoft.AspNetCore.Mvc;

namespace ASCwed.ViewComponents
{
    public class LeftNavigationViewComponent : ViewComponent
    {
        private readonly INavigationCacheOperations _navigationCacheOperations;

        public LeftNavigationViewComponent(INavigationCacheOperations navigationCacheOperations)
        {
            _navigationCacheOperations = navigationCacheOperations;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var currentUser = HttpContext.Session.GetObjectFromJson<CurrentUser>(SessionConstants.CurrentUser)
                ?? HttpContext.User.ToCurrentUser();

            var roles = currentUser.Roles.Count > 0
                ? currentUser.Roles
                : HttpContext.User.GetRoles();

            var filteredMenu = (await _navigationCacheOperations.GetMenuItemsAsync())
                .Where(item => item.IsAvailableFor(roles))
                .OrderBy(item => item.Sequence)
                .Select(item => item.FilterForRoles(roles))
                .ToList();

            return View(new LeftNavigationViewModel
            {
                CurrentUser = currentUser,
                MenuItems = filteredMenu
            });
        }
    }
}
