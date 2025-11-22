using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebAppDoCongNghe.Models.Entity;

[Table("SanPhamYeuThich")]
[Index("TaiKhoanId", "SanPhamId", Name = "UQ_YeuThich", IsUnique = true)]
public partial class SanPhamYeuThich
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("TaiKhoanID")]
    public int TaiKhoanId { get; set; }

    [Column("SanPhamID")]
    public int SanPhamId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime NgayTao { get; set; }

    [ForeignKey("SanPhamId")]
    [InverseProperty("SanPhamYeuThiches")]
    public virtual SanPham SanPham { get; set; } = null!;

    [ForeignKey("TaiKhoanId")]
    [InverseProperty("SanPhamYeuThiches")]
    public virtual TaiKhoan TaiKhoan { get; set; } = null!;
}
