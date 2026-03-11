using System;
using System.Collections.Generic;

namespace WebAppDoCongNghe.Models.Entities;

public partial class TaiKhoanPhieuGiamGium
{
    public int Id { get; set; }

    public int TaiKhoanId { get; set; }

    public int PhieuGiamGiaId { get; set; }

    public DateTime? NgayNhan { get; set; }

    public bool? DaSuDung { get; set; }

    public DateTime? NgaySuDung { get; set; }

    public virtual PhieuGiamGium PhieuGiamGia { get; set; } = null!;

    public virtual TaiKhoan TaiKhoan { get; set; } = null!;
}
