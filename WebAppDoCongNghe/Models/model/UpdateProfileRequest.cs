using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace WebAppDoCongNghe.Models.model
{
    public class UpdateProfileRequest
    {
        [Required(ErrorMessage = "Họ tên không được để trống")]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        public string? SoDienThoai { get; set; }

        public string? DiaChi { get; set; }

        public IFormFile? HinhAnh { get; set; }
    }
}

