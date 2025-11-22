using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebAppDoCongNghe.Models.Entity;

[Index("MaPhieu", Name = "UQ__PhieuGia__2660BFE18C608229", IsUnique = true)]
public partial class PhieuGiamGium
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [StringLength(50)]
    public string MaPhieu { get; set; } = null!;

    [StringLength(200)]
    public string? MoTa { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal GiaTriGiam { get; set; }

    [StringLength(10)]
    public string KieuGiam { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime NgayBatDau { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime NgayKetThuc { get; set; }

    public int? SoLuong { get; set; }

    public bool? TrangThai { get; set; }

    [InverseProperty("PhieuGiamGia")]
    public virtual ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();

    [InverseProperty("PhieuGiamGia")]
    public virtual ICollection<TaiKhoanPhieuGiamGium> TaiKhoanPhieuGiamGia { get; set; } = new List<TaiKhoanPhieuGiamGium>();
}
