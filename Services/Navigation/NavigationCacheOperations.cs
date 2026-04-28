using System.Text.Json;
using Lab8_PhamVanTung_2324801030079.Models;
using Microsoft.Extensions.Caching.Memory;

namespace Lab8_PhamVanTung_2324801030079.Services.Navigation;

public sealed class NavigationCacheOperations : INavigationCacheOperations
{
    private const string NavigationCacheKey = "LEFT_NAVIGATION_CACHE";

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

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

    public async Task<IReadOnlyList<NavigationItem>> GetNavigationAsync(CancellationToken cancellationToken = default)
    {
        if (_memoryCache.TryGetValue(NavigationCacheKey, out IReadOnlyList<NavigationItem>? navigationItems)
            && navigationItems is not null)
        {
            return navigationItems;
        }

        return await WarmUpAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<NavigationItem>> WarmUpAsync(CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(_environment.ContentRootPath, "Navigation.json");

        if (!File.Exists(filePath))
        {
            _logger.LogWarning("Navigation configuration file was not found at {NavigationFilePath}.", filePath);
            return Array.Empty<NavigationItem>();
        }

        await using var fileStream = File.OpenRead(filePath);
        var configuration = await JsonSerializer.DeserializeAsync<NavigationConfiguration>(
            fileStream,
            SerializerOptions,
            cancellationToken);

        var items = (IReadOnlyList<NavigationItem>)(configuration?.Items ?? []);

        _memoryCache.Set(NavigationCacheKey, items, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6)
        });

        _logger.LogInformation("Navigation metadata loaded successfully with {Count} root items.", items.Count);
        return items;
    }
}
