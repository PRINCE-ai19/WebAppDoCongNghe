using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebAppDoCongNghe.Models.Entity;

public partial class DanhGium
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("TaiKhoanID")]
    public int? TaiKhoanId { get; set; }

    [Column("SanPhamID")]
    public int? SanPhamId { get; set; }

    public int? SoSao { get; set; }

    [StringLength(255)]
    public string? NoiDung { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayDanhGia { get; set; }

    [ForeignKey("SanPhamId")]
    [InverseProperty("DanhGia")]
    public virtual SanPham? SanPham { get; set; }

    [ForeignKey("TaiKhoanId")]
    [InverseProperty("DanhGia")]
    public virtual TaiKhoan? TaiKhoan { get; set; }
}
