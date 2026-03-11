using System;
using System.Collections.Generic;

namespace WebAppDoCongNghe.Models.Entities;

public partial class ThongBao
{
    public int Id { get; set; }

    public int TaiKhoanId { get; set; }

    public string? TieuDe { get; set; }

    public string? NoiDung { get; set; }

    public DateTime? NgayTao { get; set; }

    public bool? DaXem { get; set; }

    public virtual TaiKhoan TaiKhoan { get; set; } = null!;
}
