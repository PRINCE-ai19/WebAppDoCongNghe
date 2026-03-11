using System;
using System.Collections.Generic;

namespace WebAppDoCongNghe.Models.Entities;

public partial class ThanhToanTam
{
    public int Id { get; set; }

    public int TaiKhoanId { get; set; }

    public decimal TongTien { get; set; }

    public string? NoiDung { get; set; }

    public string? TrangThai { get; set; }

    public DateTime? NgayTao { get; set; }

    public bool? IsVnPay { get; set; }

    public int? DonHangId { get; set; }

    public virtual DonHang? DonHang { get; set; }

    public virtual TaiKhoan TaiKhoan { get; set; } = null!;
}
