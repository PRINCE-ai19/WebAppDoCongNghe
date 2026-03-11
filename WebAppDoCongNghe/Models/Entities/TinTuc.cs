using System;
using System.Collections.Generic;

namespace WebAppDoCongNghe.Models.Entities;

public partial class TinTuc
{
    public int Id { get; set; }

    public string TieuDe { get; set; } = null!;

    public string? MoTa { get; set; }

    public string? NoiDung { get; set; }

    public string? Image { get; set; }

    public DateTime? NgayTao { get; set; }

    public DateTime? NgaySua { get; set; }

    public bool? HienThi { get; set; }
}
