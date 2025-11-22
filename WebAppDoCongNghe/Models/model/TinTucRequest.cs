using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAppDoCongNghe.Models.model
{
    public class TinTucRequest
    {
    
        [StringLength(255)]
        public string TieuDe { get; set; } = null!;

        public string? MoTa { get; set; }

        public string? NoiDung { get; set; }


        public IFormFile? Image { get; set; }


        [Column(TypeName = "datetime")]
        public DateTime? NgayTao { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? NgaySua { get; set; }

        public bool? HienThi { get; set; }
    }
}
