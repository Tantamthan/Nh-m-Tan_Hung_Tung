namespace ASC.Business.Interfaces
{
    public interface IOnlineUsersOperations
    {
        Task CreateOnlineUserAsync(string email);

        Task DeleteOnlineUserAsync(string email);

        Task<bool> GetOnlineUserAsync(string email);
    }
}
