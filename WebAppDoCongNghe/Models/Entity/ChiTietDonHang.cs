using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebAppDoCongNghe.Models.Entity;

[Table("ChiTietDonHang")]
public partial class ChiTietDonHang
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("DonHangID")]
    public int? DonHangId { get; set; }

    [Column("SanPhamID")]
    public int? SanPhamId { get; set; }

    public int? SoLuong { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? DonGia { get; set; }

    [ForeignKey("DonHangId")]
    [InverseProperty("ChiTietDonHangs")]
    public virtual DonHang? DonHang { get; set; }

    [ForeignKey("SanPhamId")]
    [InverseProperty("ChiTietDonHangs")]
    public virtual SanPham? SanPham { get; set; }
}
