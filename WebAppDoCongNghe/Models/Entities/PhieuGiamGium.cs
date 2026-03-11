using System;
using System.Collections.Generic;

namespace WebAppDoCongNghe.Models.Entities;

public partial class PhieuGiamGium
{
    public int Id { get; set; }

    public string MaPhieu { get; set; } = null!;

    public string? MoTa { get; set; }

    public decimal GiaTriGiam { get; set; }

    public string KieuGiam { get; set; } = null!;

    public DateTime NgayBatDau { get; set; }

    public DateTime NgayKetThuc { get; set; }

    public int? SoLuong { get; set; }

    public bool? TrangThai { get; set; }

    public virtual ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();

    public virtual ICollection<TaiKhoanPhieuGiamGium> TaiKhoanPhieuGiamGia { get; set; } = new List<TaiKhoanPhieuGiamGium>();
}
