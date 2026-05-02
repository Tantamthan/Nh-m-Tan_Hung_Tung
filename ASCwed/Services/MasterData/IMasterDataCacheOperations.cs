namespace ASCwed.Services.MasterData
{
    public interface IMasterDataCacheOperations
    {
        Task<MasterDataCache> GetMasterDataCacheAsync();

        Task CreateMasterDataCacheAsync();
    }
}
