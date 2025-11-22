using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebAppDoCongNghe.Models.Entity;

[Table("DonHang")]
public partial class DonHang
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("TaiKhoanID")]
    public int? TaiKhoanId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayDat { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? TongTien { get; set; }

    [StringLength(50)]
    public string? TrangThai { get; set; }

    [StringLength(255)]
    public string? DiaChiGiao { get; set; }

    [StringLength(255)]
    public string? GhiChu { get; set; }

    public bool? PhuongThucThanhToan { get; set; }

    public int? PhieuGiamGiaId { get; set; }

    [InverseProperty("DonHang")]
    public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();

    [InverseProperty("DonHang")]
    public virtual ICollection<GiaoHang> GiaoHangs { get; set; } = new List<GiaoHang>();

    [ForeignKey("PhieuGiamGiaId")]
    [InverseProperty("DonHangs")]
    public virtual PhieuGiamGium? PhieuGiamGia { get; set; }

    [ForeignKey("TaiKhoanId")]
    [InverseProperty("DonHangs")]
    public virtual TaiKhoan? TaiKhoan { get; set; }

    [InverseProperty("DonHang")]
    public virtual ICollection<ThanhToanTam> ThanhToanTams { get; set; } = new List<ThanhToanTam>();
}
