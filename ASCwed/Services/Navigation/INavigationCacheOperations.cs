using ASCwed.Models.Navigation;

namespace ASCwed.Services.Navigation
{
    public interface INavigationCacheOperations
    {
        Task<IReadOnlyCollection<NavigationMenuItem>> GetMenuItemsAsync();

        Task WarmAsync();
    }
}
