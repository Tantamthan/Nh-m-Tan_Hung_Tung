using System.ComponentModel.DataAnnotations;

namespace Lab8_PhamVanTung_2324801030079.Models
{
    public class ServiceRequestViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mô tả")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Chọn mức độ ưu tiên")]
        public string Priority { get; set; }
    }
}