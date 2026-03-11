using System;
using System.Collections.Generic;

namespace WebAppDoCongNghe.Models.Entities;

public partial class HinhAnhSanPham
{
    public int Id { get; set; }

    public int SanPhamId { get; set; }

    public string? HinhAnh { get; set; }

    public virtual SanPham SanPham { get; set; } = null!;
}
