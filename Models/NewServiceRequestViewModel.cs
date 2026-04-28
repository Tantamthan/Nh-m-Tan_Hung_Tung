using System.ComponentModel.DataAnnotations;
using Lab8_PhamVanTung_2324801030079.Models;
namespace Lab8_PhamVanTung_2324801030079.Models
{
    public class NewServiceRequestViewModel
    {
        [Required]
        public string Title { get; set; } = "";

        [Required]
        public string Description { get; set; } = "";

        [Required]
        public string Priority { get; set; } = "";
    }
}