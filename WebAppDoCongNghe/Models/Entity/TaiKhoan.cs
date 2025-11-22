using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebAppDoCongNghe.Models.Entity;

[Table("TaiKhoan")]
[Index("Email", Name = "UQ__TaiKhoan__A9D105345659533D", IsUnique = true)]
public partial class TaiKhoan
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [StringLength(100)]
    public string HoTen { get; set; } = null!;

    [StringLength(100)]
    public string Email { get; set; } = null!;

    [StringLength(255)]
    public string MatKhau { get; set; } = null!;

    [StringLength(20)]
    public string? SoDienThoai { get; set; }

    [StringLength(255)]
    public string? DiaChi { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayDangKy { get; set; }

    public int? IdLoaiTaiKhoan { get; set; }

    [Column("OTP")]
    [StringLength(10)]
    public string? Otp { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? OtpExpire { get; set; }

    [InverseProperty("TaiKhoan")]
    public virtual ICollection<DanhGium> DanhGia { get; set; } = new List<DanhGium>();

    [InverseProperty("TaiKhoan")]
    public virtual ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();

    [InverseProperty("TaiKhoan")]
    public virtual ICollection<GioHang> GioHangs { get; set; } = new List<GioHang>();

    [ForeignKey("IdLoaiTaiKhoan")]
    [InverseProperty("TaiKhoans")]
    public virtual LoaiTaiKhoan? IdLoaiTaiKhoanNavigation { get; set; }

    [InverseProperty("TaiKhoan")]
    public virtual ICollection<SanPhamYeuThich> SanPhamYeuThiches { get; set; } = new List<SanPhamYeuThich>();

    [InverseProperty("TaiKhoan")]
    public virtual ICollection<TaiKhoanPhieuGiamGium> TaiKhoanPhieuGiamGia { get; set; } = new List<TaiKhoanPhieuGiamGium>();

    [InverseProperty("TaiKhoan")]
    public virtual ICollection<ThanhToanTam> ThanhToanTams { get; set; } = new List<ThanhToanTam>();

    [InverseProperty("TaiKhoan")]
    public virtual ICollection<ThongBao> ThongBaos { get; set; } = new List<ThongBao>();
}
