using System;
using System.Collections.Generic;

namespace WebAppDoCongNghe.Models.Entities;

public partial class PhanQuyen
{
    public int Id { get; set; }

    public string? MaChucNang { get; set; }

    public string? MoTa { get; set; }

    public virtual ICollection<TaiKhoanPhanQuyen> TaiKhoanPhanQuyens { get; set; } = new List<TaiKhoanPhanQuyen>();
}
