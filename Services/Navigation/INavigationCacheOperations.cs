using Lab8_PhamVanTung_2324801030079.Models;

namespace Lab8_PhamVanTung_2324801030079.Services.Navigation;

public interface INavigationCacheOperations
{
    Task<IReadOnlyList<NavigationItem>> GetNavigationAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NavigationItem>> WarmUpAsync(CancellationToken cancellationToken = default);
}
