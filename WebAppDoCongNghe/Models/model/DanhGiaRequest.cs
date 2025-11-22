using System.ComponentModel.DataAnnotations;

namespace WebAppDoCongNghe.Models.model
{
    public class DanhGiaRequest
    {
        [Required(ErrorMessage = "Mã sản phẩm là bắt buộc")]
        public int SanPhamId { get; set; }

        [Required(ErrorMessage = "Số sao là bắt buộc")]
        [Range(1, 5, ErrorMessage = "Số sao phải từ 1 đến 5")]
        public int SoSao { get; set; }

        [StringLength(255, ErrorMessage = "Nội dung đánh giá không được vượt quá 255 ký tự")]
        public string? NoiDung { get; set; }
    }
}

