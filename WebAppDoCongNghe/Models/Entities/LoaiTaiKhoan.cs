using System;
using System.Collections.Generic;

namespace WebAppDoCongNghe.Models.Entities;

public partial class LoaiTaiKhoan
{
    public int Id { get; set; }

    public string TenLoaiTaiKhoan { get; set; } = null!;

    public virtual ICollection<TaiKhoan> TaiKhoans { get; set; } = new List<TaiKhoan>();
}
