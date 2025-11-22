    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    namespace WebAppDoCongNghe.Models.model
    {
        public class SannPhamBind
        {
            public int Id { get; set; } 

            [StringLength(100)]
            public string TenSanPham { get; set; }

            public int? DanhMucId { get; set; }

       
            [StringLength(100)]
            public string? ThuongHieu { get; set; }

            public List<IFormFile>? Hinhanh { get; set; }


            [Column(TypeName = "decimal(18, 2)")]
            public decimal Gia { get; set; }

            public int? SoLuongTon { get; set; }

           

            public string? MoTa { get; set; }

            [Column(TypeName = "datetime")]
            public DateTime? NgayThem { get; set; }

/*
            [ForeignKey("DanhMucId")]
            [InverseProperty("SanPhams")]
            public virtual DanhMuc? DanhMuc { get; set; }*/

        }
    }
