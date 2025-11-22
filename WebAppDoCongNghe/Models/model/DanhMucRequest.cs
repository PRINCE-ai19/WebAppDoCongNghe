using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAppDoCongNghe.Models.model
{
    public class DanhMucRequest
    {
      

        [StringLength(100)]
        public string TenDanhMuc { get; set; } = null!;

        [StringLength(255)]
        public string? MoTa { get; set; }

        public int? ViTri { get; set; }
    }
}
