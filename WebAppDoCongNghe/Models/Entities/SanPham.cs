using System;
using System.Collections.Generic;

namespace WebAppDoCongNghe.Models.Entities;

public partial class SanPham
{
    public int Id { get; set; }

    public string TenSanPham { get; set; } = null!;

    public int? DanhMucId { get; set; }

    public string? ThuongHieu { get; set; }

    public decimal Gia { get; set; }

    public decimal? GiaGiam { get; set; }

    public int? SoLuongTon { get; set; }

    public string? MoTa { get; set; }

    public DateTime? NgayThem { get; set; }

    public bool? HienThi { get; set; }

    public virtual ICollection<CauHinhSanPham> CauHinhSanPhams { get; set; } = new List<CauHinhSanPham>();

    public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();

    public virtual ICollection<ChiTietGioHang> ChiTietGioHangs { get; set; } = new List<ChiTietGioHang>();

    public virtual ICollection<DanhGium> DanhGia { get; set; } = new List<DanhGium>();

    public virtual DanhMuc? DanhMuc { get; set; }

    public virtual ICollection<HinhAnhSanPham> HinhAnhSanPhams { get; set; } = new List<HinhAnhSanPham>();

    public virtual ICollection<SanPhamKhuyenMai> SanPhamKhuyenMais { get; set; } = new List<SanPhamKhuyenMai>();

    public virtual ICollection<SanPhamYeuThich> SanPhamYeuThiches { get; set; } = new List<SanPhamYeuThich>();
}
