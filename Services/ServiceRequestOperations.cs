using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lab8_PhamVanTung_2324801030079.Models;

namespace Lab8_PhamVanTung_2324801030079.Services
{
    public class ServiceRequestOperations : IServiceRequestOperations
    {
        private static List<ServiceRequest> _data = new List<ServiceRequest>();

        public Task CreateAsync(ServiceRequest request)
        {
            _data.Add(request);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<ServiceRequest>> GetAllAsync()
        {
            return Task.FromResult(_data.AsEnumerable());
        }
    }
}