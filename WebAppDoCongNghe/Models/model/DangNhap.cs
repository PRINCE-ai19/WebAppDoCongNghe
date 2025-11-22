using System.ComponentModel.DataAnnotations;

namespace WebAppDoCongNghe.Models.model
{
    public class DangNhap
    {
        [Required(ErrorMessage = "Email hoặc số điện thoại không được để trống")]
        [StringLength(100, ErrorMessage = "Tối đa 100 ký tự")]
        public string TaiKhoan { get; set; }


        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 đến 50 ký tự")]
        public string MatKhau { get; set; } 

    }
}
