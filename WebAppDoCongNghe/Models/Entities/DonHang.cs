using System;
using System.Collections.Generic;

namespace WebAppDoCongNghe.Models.Entities;

public partial class DonHang
{
    public int Id { get; set; }

    public int? TaiKhoanId { get; set; }

    public DateTime? NgayDat { get; set; }

    public decimal? TongTien { get; set; }

    public string? TrangThai { get; set; }

    public string? DiaChiGiao { get; set; }

    public string? GhiChu { get; set; }

    public bool? PhuongThucThanhToan { get; set; }

    public int? PhieuGiamGiaId { get; set; }

    public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();

    public virtual ICollection<GiaoHang> GiaoHangs { get; set; } = new List<GiaoHang>();

    public virtual PhieuGiamGium? PhieuGiamGia { get; set; }

    public virtual TaiKhoan? TaiKhoan { get; set; }

    public virtual ICollection<ThanhToanTam> ThanhToanTams { get; set; } = new List<ThanhToanTam>();
}
