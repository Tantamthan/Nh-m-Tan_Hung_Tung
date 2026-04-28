using Lab8_PhamVanTung_2324801030079.Models.Cache;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Lab8_PhamVanTung_2324801030079.Business.Cache
{
    public class MasterDataCacheOperations : IMasterDataCacheOperations
    {
        private readonly IDistributedCache _cache;

        public MasterDataCacheOperations(IDistributedCache cache)
        {
            _cache = cache;
        }


        public async Task CreateMasterDataCacheAsync()
        {
         
            await Task.CompletedTask;
        }

        public async Task SetMasterDataAsync(string key, object data)
        {
            var jsonData = JsonConvert.SerializeObject(data);
            var bytes = Encoding.UTF8.GetBytes(jsonData);

            var options = new DistributedCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(30));

            await _cache.SetAsync(key, bytes, options);
        }


        public async Task<T?> GetMasterDataAsync<T>(string key)
        {
            var data = await _cache.GetAsync(key);

            if (data == null)
                return default;

            var jsonData = Encoding.UTF8.GetString(data);

            return JsonConvert.DeserializeObject<T>(jsonData);
        }

        public async Task RemoveMasterDataAsync(string key)
        {
            await _cache.RemoveAsync(key);
        }
    }
}