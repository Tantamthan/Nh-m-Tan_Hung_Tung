using System.ComponentModel.DataAnnotations;

namespace ASCwed.Areas.Configuration.Models
{
    public class MasterDataKeyViewModel
    {
        public string? RowKey { get; set; }

        public string? PartitionKey { get; set; }

        public bool IsActive { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên master key.")]
        public string Name { get; set; } = string.Empty;
    }
}
