using ASCwed.Models.Navigation;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace ASCwed.Services.Navigation
{
    public class NavigationCacheOperations : INavigationCacheOperations
    {
        private const string NavigationCacheKey = "ASCwed.Navigation";
        private readonly IMemoryCache _memoryCache;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<NavigationCacheOperations> _logger;

        public NavigationCacheOperations(
            IMemoryCache memoryCache,
            IWebHostEnvironment environment,
            ILogger<NavigationCacheOperations> logger)
        {
            _memoryCache = memoryCache;
            _environment = environment;
            _logger = logger;
        }

        public async Task<IReadOnlyCollection<NavigationMenuItem>> GetMenuItemsAsync()
        {
            return await _memoryCache.GetOrCreateAsync(NavigationCacheKey, async cacheEntry =>
            {
                cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6);
                return await LoadNavigationAsync();
            }) ?? [];
        }

        public async Task WarmAsync()
        {
            _ = await GetMenuItemsAsync();
        }

        private async Task<IReadOnlyCollection<NavigationMenuItem>> LoadNavigationAsync()
        {
            var navigationFile = Path.Combine(_environment.ContentRootPath, "Navigation.json");

            if (!File.Exists(navigationFile))
            {
                _logger.LogWarning("Navigation file was not found at {NavigationFile}", navigationFile);
                return [];
            }

            await using var stream = File.OpenRead(navigationFile);
            var navigationConfiguration = await JsonSerializer.DeserializeAsync<NavigationConfiguration>(
                stream,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            return navigationConfiguration?.MenuItems
                .OrderBy(item => item.Sequence)
                .ToList() ?? [];
        }
    }
}
