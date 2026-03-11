using System;
using System.Collections.Generic;

namespace WebAppDoCongNghe.Models.Entities;

public partial class CauHinhSanPham
{
    public int Id { get; set; }

    public int SanPhamId { get; set; }

    public string? TenThongSo { get; set; }

    public string? GiaTri { get; set; }

    public virtual SanPham SanPham { get; set; } = null!;
}
