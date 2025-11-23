using System.ComponentModel.DataAnnotations;

namespace WebAppDoCongNghe.Models.model
{
    public class GanSanPhamKhuyenMaiRequest
    {
        [Required(ErrorMessage = "ID sản phẩm không được để trống")]
        public int SanPhamId { get; set; }

        [Required(ErrorMessage = "ID khuyến mãi không được để trống")]
        public int KhuyenMaiId { get; set; }
    }
}

