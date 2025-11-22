using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAppDoCongNghe.Models.model
{
    public class GioHangBind
    {

        [Column("TaiKhoanID")]
        public int? TaiKhoanId { get; set; }


        public int? sanPhamId { get; set; }

        public int? SoLuong { get; set; }
    }
}
