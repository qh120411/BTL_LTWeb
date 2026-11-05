using System.ComponentModel.DataAnnotations;

namespace BTL_LTWEB.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập hoặc mã sinh viên")]
        [Display(Name = "Tên đăng nhập / Mã sinh viên")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Ghi nhớ đăng nhập")]
        public bool RememberMe { get; set; } = false;

        public string? ReturnUrl { get; set; }
    }
}

