using System;
using System.Collections.Generic;

namespace WebAppDoCongNghe.Models.Entities;

public partial class DanhGium
{
    public int Id { get; set; }

    public int? TaiKhoanId { get; set; }

    public int? SanPhamId { get; set; }

    public int? SoSao { get; set; }

    public string? NoiDung { get; set; }

    public DateTime? NgayDanhGia { get; set; }

    public virtual SanPham? SanPham { get; set; }

    public virtual TaiKhoan? TaiKhoan { get; set; }
}
