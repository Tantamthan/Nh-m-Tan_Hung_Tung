using System.ComponentModel.DataAnnotations;

namespace ASCwed.Areas.Accounts.Models
{
    public class ProfileViewModel
    {
        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
        [StringLength(50, ErrorMessage = "Tên đăng nhập tối đa {1} ký tự.")]
        [Display(Name = "Tên đăng nhập")]
        public string UserName { get; set; } = string.Empty;

        [Display(Name = "Email (ReadOnly)")]
        public string Email { get; set; } = string.Empty;

        public bool IsEditSuccess { get; set; }
    }
}
