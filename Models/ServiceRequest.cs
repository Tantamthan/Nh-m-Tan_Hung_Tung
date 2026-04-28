using Microsoft.EntityFrameworkCore;

namespace Lab8_PhamVanTung_2324801030079.Models
{
    public class ServiceRequest
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string Priority { get; set; } = "";
        public string Status { get; set; } = "New";
        public DbSet<ServiceRequest> ServiceRequests { get; set; }
        public string CustomerId { get; set; }
    
    }
}