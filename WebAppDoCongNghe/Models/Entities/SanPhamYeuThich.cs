using System;
using System.Collections.Generic;

namespace WebAppDoCongNghe.Models.Entities;

public partial class SanPhamYeuThich
{
    public int Id { get; set; }

    public int TaiKhoanId { get; set; }

    public int SanPhamId { get; set; }

    public DateTime NgayTao { get; set; }

    public virtual SanPham SanPham { get; set; } = null!;

    public virtual TaiKhoan TaiKhoan { get; set; } = null!;
}
