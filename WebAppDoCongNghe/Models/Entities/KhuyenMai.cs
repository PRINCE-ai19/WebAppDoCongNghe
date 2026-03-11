using System;
using System.Collections.Generic;

namespace WebAppDoCongNghe.Models.Entities;

public partial class KhuyenMai
{
    public int Id { get; set; }

    public string? TenKhuyenMai { get; set; }

    public string? MoTa { get; set; }

    public decimal? PhanTramGiam { get; set; }

    public DateOnly? NgayBatDau { get; set; }

    public DateOnly? NgayKetThuc { get; set; }

    public virtual ICollection<SanPhamKhuyenMai> SanPhamKhuyenMais { get; set; } = new List<SanPhamKhuyenMai>();
}
