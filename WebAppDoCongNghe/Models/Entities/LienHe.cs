using System;
using System.Collections.Generic;

namespace WebAppDoCongNghe.Models.Entities;

public partial class LienHe
{
    public int Id { get; set; }

    public string? HoTen { get; set; }

    public string? Email { get; set; }

    public string? SoDienThoai { get; set; }

    public string? NoiDung { get; set; }

    public DateTime? NgayGui { get; set; }
}
