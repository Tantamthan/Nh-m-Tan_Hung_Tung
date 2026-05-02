using ASC.Model.Models;

namespace ASC.Business.Interfaces
{
    public interface IServiceRequestMessageOperations
    {
        Task CreateServiceRequestMessageAsync(ServiceRequestMessage message);

        Task<List<ServiceRequestMessage>> GetServiceRequestMessageAsync(string serviceRequestId);
    }
}
