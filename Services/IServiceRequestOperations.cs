using System.Collections.Generic;
using System.Threading.Tasks;
using Lab8_PhamVanTung_2324801030079.Models;

namespace Lab8_PhamVanTung_2324801030079.Services
{
    public interface IServiceRequestOperations
    {
        Task CreateAsync(ServiceRequest request);
        Task<IEnumerable<ServiceRequest>> GetAllAsync();
   
       
    }
}