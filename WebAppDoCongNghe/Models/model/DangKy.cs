using System.ComponentModel.DataAnnotations;

namespace WebAppDoCongNghe.Models.model
{
    public class DangKy
    {
        [StringLength(100)]
        public string HoTen { get; set; }

        [RegularExpression(@"^(0|\+84)(\d{9,10})$", ErrorMessage = "Số điện thoại không hợp lệ (phải có 10 hoặc 11 số)")]
        public string? SoDienThoai { get; set; }

        [Required(ErrorMessage = "Email không được để trống")]
        [StringLength(100, ErrorMessage = "Email tối đa 100 ký tự")]
        [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ")]
        public string Email { get; set; }


        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 đến 50 ký tự")]
        public string MatKhau { get; set; }

        [Required(ErrorMessage = "Xác nhận mật khẩu không được để trống")]
        [Compare("MatKhau", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string XacNhanMatKhau { get; set; }
    }
}
