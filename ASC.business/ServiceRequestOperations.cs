using ASC.Business.Interfaces;
using ASC.DataAccess.Interfaces;
using ASC.Model.Models;
using ASC.Model.Queries;

namespace ASC.Business
{
    public class ServiceRequestOperations : IServiceRequestOperations
    {
        private readonly IUnitOfWork _unitOfWork;

        public ServiceRequestOperations(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task CreateServiceRequestAsync(ServiceRequest request)
        {
            using (_unitOfWork)
            {
                await _unitOfWork.Repository<ServiceRequest>().AddAsync(request);
                _unitOfWork.CommitTransaction();
            }
        }

        public ServiceRequest UpdateServiceRequest(ServiceRequest request)
        {
            using (_unitOfWork)
            {
                _unitOfWork.Repository<ServiceRequest>().Update(request);
                _unitOfWork.CommitTransaction();
                return request;
            }
        }

        public async Task<ServiceRequest?> GetServiceRequestAsync(string partitionKey, string rowKey)
        {
            if (string.IsNullOrWhiteSpace(partitionKey) || string.IsNullOrWhiteSpace(rowKey))
            {
                return null;
            }

            var serviceRequest = await _unitOfWork.Repository<ServiceRequest>()
                .FindAsync(partitionKey.Trim(), rowKey.Trim());

            return serviceRequest?.IsDeleted == true ? null : serviceRequest;
        }

        public async Task<ServiceRequest> UpdateServiceRequestStatusAsync(string rowKey, string partitionKey, string status)
        {
            using (_unitOfWork)
            {
                var serviceRequest = await _unitOfWork.Repository<ServiceRequest>().FindAsync(partitionKey, rowKey);
                if (serviceRequest == null)
                {
                    throw new NullReferenceException();
                }

                // Chỉ cập nhật trạng thái để giữ nguyên dữ liệu yêu cầu ban đầu của khách hàng.
                serviceRequest.Status = status;
                _unitOfWork.Repository<ServiceRequest>().Update(serviceRequest);
                _unitOfWork.CommitTransaction();
                return serviceRequest;
            }
        }

        public async Task<List<ServiceRequest>> GetServiceRequestsByRequestedDateAndStatus(
            DateTime? requestedDate,
            List<string>? status = null,
            string email = "",
            string serviceEngineerEmail = "",
            bool includeUnassignedEngineerRequests = false)
        {
            var query = Queries.GetDashboardQuery(
                requestedDate,
                status,
                email,
                serviceEngineerEmail,
                includeUnassignedEngineerRequests);
            var serviceRequests = await _unitOfWork.Repository<ServiceRequest>().FindAllByQuery(query);
            return serviceRequests.ToList();
        }
    }
}
