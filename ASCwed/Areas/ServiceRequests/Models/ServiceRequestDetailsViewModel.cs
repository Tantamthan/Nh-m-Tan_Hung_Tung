using ASC.Model.Models;

namespace ASCwed.Areas.ServiceRequests.Models
{
    public class ServiceRequestDetailsViewModel
    {
        public ServiceRequest ServiceRequest { get; set; } = new();

        public List<ServiceRequestMessage> Messages { get; set; } = [];
    }
}
