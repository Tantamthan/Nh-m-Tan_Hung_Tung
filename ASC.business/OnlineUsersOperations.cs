using ASC.Business.Interfaces;
using ASC.DataAccess.Interfaces;
using ASC.Model.Models;

namespace ASC.Business
{
    public class OnlineUsersOperations : IOnlineUsersOperations
    {
        private readonly IUnitOfWork _unitOfWork;

        public OnlineUsersOperations(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task CreateOnlineUserAsync(string email)
        {
            var normalizedEmail = NormalizeEmail(email);
            if (string.IsNullOrWhiteSpace(normalizedEmail))
            {
                return;
            }

            var users = await _unitOfWork.Repository<OnlineUser>()
                .FindAllByPartitionKeyAsync(normalizedEmail);
            var onlineUser = users.FirstOrDefault();

            if (onlineUser == null)
            {
                onlineUser = new OnlineUser(normalizedEmail)
                {
                    IsDeleted = false,
                    CreatedBy = normalizedEmail,
                    UpdatedBy = normalizedEmail
                };

                await _unitOfWork.Repository<OnlineUser>().AddAsync(onlineUser);
            }
            else
            {
                // Đánh dấu user online lại trên bản ghi hiện có để tránh tạo trùng theo email.
                onlineUser.IsDeleted = false;
                onlineUser.UpdatedBy = normalizedEmail;
                _unitOfWork.Repository<OnlineUser>().Update(onlineUser);
            }

            _unitOfWork.CommitTransaction();
        }

        public async Task DeleteOnlineUserAsync(string email)
        {
            var normalizedEmail = NormalizeEmail(email);
            if (string.IsNullOrWhiteSpace(normalizedEmail))
            {
                return;
            }

            var users = await _unitOfWork.Repository<OnlineUser>()
                .FindAllByPartitionKeyAsync(normalizedEmail);
            var onlineUser = users.FirstOrDefault(user => !user.IsDeleted);

            if (onlineUser == null)
            {
                return;
            }

            // Giữ record và chỉ đổi trạng thái để lần kết nối sau có thể tái sử dụng.
            onlineUser.IsDeleted = true;
            onlineUser.UpdatedBy = normalizedEmail;
            _unitOfWork.Repository<OnlineUser>().Update(onlineUser);
            _unitOfWork.CommitTransaction();
        }

        public async Task<bool> GetOnlineUserAsync(string email)
        {
            var normalizedEmail = NormalizeEmail(email);
            if (string.IsNullOrWhiteSpace(normalizedEmail))
            {
                return false;
            }

            var users = await _unitOfWork.Repository<OnlineUser>()
                .FindAllByPartitionKeyAsync(normalizedEmail);

            return users.Any(user => !user.IsDeleted);
        }

        private static string NormalizeEmail(string email)
        {
            return email?.Trim() ?? string.Empty;
        }
    }
}
