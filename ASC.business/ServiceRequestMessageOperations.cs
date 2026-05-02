using ASC.Business.Interfaces;
using ASC.DataAccess.Interfaces;
using ASC.Model.Models;

namespace ASC.Business
{
    public class ServiceRequestMessageOperations : IServiceRequestMessageOperations
    {
        private readonly IUnitOfWork _unitOfWork;

        public ServiceRequestMessageOperations(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task CreateServiceRequestMessageAsync(ServiceRequestMessage message)
        {
            NormalizeMessage(message);

            await _unitOfWork.Repository<ServiceRequestMessage>().AddAsync(message);
            _unitOfWork.CommitTransaction();
        }

        public async Task<List<ServiceRequestMessage>> GetServiceRequestMessageAsync(string serviceRequestId)
        {
            if (string.IsNullOrWhiteSpace(serviceRequestId))
            {
                return new List<ServiceRequestMessage>();
            }

            var messages = await _unitOfWork.Repository<ServiceRequestMessage>()
                .FindAllByPartitionKeyAsync(serviceRequestId.Trim());

            return messages
                .Where(message => !message.IsDeleted)
                .OrderBy(message => message.MessageDate ?? message.CreatedDate)
                .ToList();
        }

        private static void NormalizeMessage(ServiceRequestMessage message)
        {
            message.PartitionKey = message.PartitionKey?.Trim() ?? string.Empty;
            message.RowKey = string.IsNullOrWhiteSpace(message.RowKey) ? Guid.NewGuid().ToString() : message.RowKey.Trim();
            message.FromDisplayName = message.FromDisplayName?.Trim() ?? string.Empty;
            message.FromEmail = message.FromEmail?.Trim() ?? string.Empty;
            message.Message = message.Message?.Trim() ?? string.Empty;
            message.MessageDate ??= DateTime.UtcNow;
            message.CreatedBy ??= message.FromEmail;
            message.UpdatedBy ??= message.CreatedBy;
        }
    }
}
