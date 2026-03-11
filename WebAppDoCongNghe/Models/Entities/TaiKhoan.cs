using System;
using System.Collections.Generic;

namespace WebAppDoCongNghe.Models.Entities;

public partial class TaiKhoan
{
    public int Id { get; set; }

    public string HoTen { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string MatKhau { get; set; } = null!;

    public string? SoDienThoai { get; set; }

    public string? DiaChi { get; set; }

    public DateTime? NgayDangKy { get; set; }

    public int? IdLoaiTaiKhoan { get; set; }

    public string? Otp { get; set; }

    public DateTime? OtpExpire { get; set; }

    public string? HinhAnh { get; set; }

    public virtual ICollection<DanhGium> DanhGia { get; set; } = new List<DanhGium>();

    public virtual ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();

    public virtual ICollection<GioHang> GioHangs { get; set; } = new List<GioHang>();

    public virtual LoaiTaiKhoan? IdLoaiTaiKhoanNavigation { get; set; }

    public virtual ICollection<SanPhamYeuThich> SanPhamYeuThiches { get; set; } = new List<SanPhamYeuThich>();

    public virtual ICollection<TaiKhoanPhanQuyen> TaiKhoanPhanQuyens { get; set; } = new List<TaiKhoanPhanQuyen>();

    public virtual ICollection<TaiKhoanPhieuGiamGium> TaiKhoanPhieuGiamGia { get; set; } = new List<TaiKhoanPhieuGiamGium>();

    public virtual ICollection<ThanhToanTam> ThanhToanTams { get; set; } = new List<ThanhToanTam>();

    public virtual ICollection<ThongBao> ThongBaos { get; set; } = new List<ThongBao>();
}
