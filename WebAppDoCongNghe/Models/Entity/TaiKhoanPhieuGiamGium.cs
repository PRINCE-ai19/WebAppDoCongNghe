using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebAppDoCongNghe.Models.Entity;

[Table("TaiKhoan_PhieuGiamGia")]
[Index("TaiKhoanId", "PhieuGiamGiaId", Name = "UQ_TaiKhoan_PhieuGiamGia", IsUnique = true)]
public partial class TaiKhoanPhieuGiamGium
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("TaiKhoanID")]
    public int TaiKhoanId { get; set; }

    [Column("PhieuGiamGiaID")]
    public int PhieuGiamGiaId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayNhan { get; set; }

    public bool? DaSuDung { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgaySuDung { get; set; }

    [ForeignKey("PhieuGiamGiaId")]
    [InverseProperty("TaiKhoanPhieuGiamGia")]
    public virtual PhieuGiamGium PhieuGiamGia { get; set; } = null!;

    [ForeignKey("TaiKhoanId")]
    [InverseProperty("TaiKhoanPhieuGiamGia")]
    public virtual TaiKhoan TaiKhoan { get; set; } = null!;
}
