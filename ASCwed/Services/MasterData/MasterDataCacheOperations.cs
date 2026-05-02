using ASC.Business.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace ASCwed.Services.MasterData
{
    public class MasterDataCacheOperations : IMasterDataCacheOperations
    {
        private const string MasterDataCacheName = "MasterDataCache";
        private readonly IDistributedCache _cache;
        private readonly IMasterDataOperations _masterData;

        public MasterDataCacheOperations(IDistributedCache cache, IMasterDataOperations masterData)
        {
            _cache = cache;
            _masterData = masterData;
        }

        public async Task CreateMasterDataCacheAsync()
        {
            var masterDataCache = new MasterDataCache
            {
                // Chỉ lưu dữ liệu MasterData đang active vào Redis theo hướng dẫn Lab 7.
                Keys = (await _masterData.GetAllMasterKeysAsync())
                    .Where(item => item.IsActive)
                    .ToList(),
                Values = (await _masterData.GetAllMasterValuesAsync())
                    .Where(item => item.IsActive)
                    .ToList()
            };

            await _cache.SetStringAsync(
                MasterDataCacheName,
                JsonSerializer.Serialize(masterDataCache));
        }

        public async Task<MasterDataCache> GetMasterDataCacheAsync()
        {
            var cacheContent = await _cache.GetStringAsync(MasterDataCacheName);
            if (string.IsNullOrWhiteSpace(cacheContent))
            {
                await CreateMasterDataCacheAsync();
                cacheContent = await _cache.GetStringAsync(MasterDataCacheName);
            }

            return string.IsNullOrWhiteSpace(cacheContent)
                ? new MasterDataCache()
                : JsonSerializer.Deserialize<MasterDataCache>(cacheContent) ?? new MasterDataCache();
        }
    }
}
