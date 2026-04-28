namespace Lab8_PhamVanTung_2324801030079.Models.Cache

{
    public interface IMasterDataCacheOperations
    {
        Task SetMasterDataAsync(string key, object data);
        Task<T> GetMasterDataAsync<T>(string key);
        Task RemoveMasterDataAsync(string key);
        Task CreateMasterDataCacheAsync();
    }
}