using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebAppDoCongNghe.Models.Entity;

[Table("ChiTietGioHang")]
public partial class ChiTietGioHang
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("GioHangID")]
    public int? GioHangId { get; set; }

    [Column("SanPhamID")]
    public int? SanPhamId { get; set; }

    public int? SoLuong { get; set; }

    [ForeignKey("GioHangId")]
    [InverseProperty("ChiTietGioHangs")]
    public virtual GioHang? GioHang { get; set; }

    [ForeignKey("SanPhamId")]
    [InverseProperty("ChiTietGioHangs")]
    public virtual SanPham? SanPham { get; set; }
}
