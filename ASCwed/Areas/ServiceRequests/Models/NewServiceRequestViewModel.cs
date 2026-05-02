using System.ComponentModel.DataAnnotations;

namespace ASCwed.Areas.ServiceRequests.Models
{
    public class NewServiceRequestViewModel
    {
        [Required]
        [Display(Name = "Vehicle Name")]
        public string VehicleName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Vehicle Type")]
        public string VehicleType { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Requested Services")]
        public string RequestedServices { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Requested Date")]
        public DateTime? RequestedDate { get; set; }
    }
}
