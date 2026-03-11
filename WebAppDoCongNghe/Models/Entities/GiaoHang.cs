using System;
using System.Collections.Generic;

namespace WebAppDoCongNghe.Models.Entities;

public partial class GiaoHang
{
    public int Id { get; set; }

    public int? DonHangId { get; set; }

    public string? TrangThai { get; set; }

    public DateTime? NgayCapNhat { get; set; }

    public string? DonViVanChuyen { get; set; }

    public virtual DonHang? DonHang { get; set; }
}
