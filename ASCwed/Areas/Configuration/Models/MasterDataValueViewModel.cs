using System.ComponentModel.DataAnnotations;

namespace ASCwed.Areas.Configuration.Models
{
    public class MasterDataValueViewModel
    {
        public string? RowKey { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn master key.")]
        [Display(Name = "Partition Key")]
        public string PartitionKey { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập master value.")]
        public string Name { get; set; } = string.Empty;
    }
}
