using System;
using System.Collections.Generic;

namespace WebAppDoCongNghe.Models.Entities;

public partial class TaiKhoanPhanQuyen
{
    public int Id { get; set; }

    public int IdChucNang { get; set; }

    public bool? QuyenXem { get; set; }

    public bool? QuyenThem { get; set; }

    public bool? QuyenSua { get; set; }

    public bool? QuyenXoa { get; set; }

    public int IdTaiKhoan { get; set; }

    public virtual PhanQuyen IdChucNangNavigation { get; set; } = null!;

    public virtual TaiKhoan IdTaiKhoanNavigation { get; set; } = null!;
}
