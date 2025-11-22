using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebAppDoCongNghe.Models.Entity;

[Table("SanPham")]
public partial class SanPham
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [StringLength(100)]
    public string TenSanPham { get; set; } = null!;

    [Column("DanhMucID")]
    public int? DanhMucId { get; set; }

    [StringLength(100)]
    public string? ThuongHieu { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Gia { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? GiaGiam { get; set; }

    public int? SoLuongTon { get; set; }

    public string? MoTa { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayThem { get; set; }

    public bool? HienThi { get; set; }

    [InverseProperty("SanPham")]
    public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();

    [InverseProperty("SanPham")]
    public virtual ICollection<ChiTietGioHang> ChiTietGioHangs { get; set; } = new List<ChiTietGioHang>();

    [InverseProperty("SanPham")]
    public virtual ICollection<DanhGium> DanhGia { get; set; } = new List<DanhGium>();

    [ForeignKey("DanhMucId")]
    [InverseProperty("SanPhams")]
    public virtual DanhMuc? DanhMuc { get; set; }

    [InverseProperty("SanPham")]
    public virtual ICollection<HinhAnhSanPham> HinhAnhSanPhams { get; set; } = new List<HinhAnhSanPham>();

    [InverseProperty("SanPham")]
    public virtual ICollection<SanPhamKhuyenMai> SanPhamKhuyenMais { get; set; } = new List<SanPhamKhuyenMai>();

    [InverseProperty("SanPham")]
    public virtual ICollection<SanPhamYeuThich> SanPhamYeuThiches { get; set; } = new List<SanPhamYeuThich>();
}
