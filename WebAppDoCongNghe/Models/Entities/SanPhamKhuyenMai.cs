using System;
using System.Collections.Generic;

namespace WebAppDoCongNghe.Models.Entities;

public partial class SanPhamKhuyenMai
{
    public int Id { get; set; }

    public int? SanPhamId { get; set; }

    public int? KhuyenMaiId { get; set; }

    public virtual KhuyenMai? KhuyenMai { get; set; }

    public virtual SanPham? SanPham { get; set; }
}
