using System.ComponentModel.DataAnnotations;

namespace ASCwed.Areas.Accounts.Models
{
    public class ServiceEngineerRegistrationViewModel
    {
        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
        [Display(Name = "Tên đăng nhập")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [StringLength(100, ErrorMessage = "Mật khẩu phải có ít nhất {2} ký tự.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        [Display(Name = "Xác nhận mật khẩu")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Kích hoạt tài khoản")]
        public bool IsActive { get; set; } = true;
    }

    public class ServiceEngineerUpdateViewModel
    {
        [Required]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Kích hoạt tài khoản")]
        public bool IsActive { get; set; }
    }

    public class ServiceEngineersViewModel
    {
        public ServiceEngineerRegistrationViewModel Registration { get; set; }
        public List<ServiceEngineerUpdateViewModel> ServiceEngineers { get; set; }
    }
}
