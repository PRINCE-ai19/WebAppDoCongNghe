using System.ComponentModel.DataAnnotations;

namespace WebAppDoCongNghe.Models.model
{
    public class TaiKhoanRequest
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }


        public string? SoDienThoai { get; set; }

        public string? DiaChi { get; set; }

        public DateTime? NgayDangKy { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn loại tài khoản")]
        public int IdLoaiTaiKhoan { get; set; }
    }
}
