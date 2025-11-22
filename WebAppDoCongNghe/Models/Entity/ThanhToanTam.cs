using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebAppDoCongNghe.Models.Entity;

[Table("ThanhToanTam")]
public partial class ThanhToanTam
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("TaiKhoanID")]
    public int TaiKhoanId { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TongTien { get; set; }

    [StringLength(255)]
    public string? NoiDung { get; set; }

    [StringLength(50)]
    public string? TrangThai { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayTao { get; set; }

    public bool? IsVnPay { get; set; }

    [Column("DonHangID")]
    public int? DonHangId { get; set; }

    [ForeignKey("DonHangId")]
    [InverseProperty("ThanhToanTams")]
    public virtual DonHang? DonHang { get; set; }

    [ForeignKey("TaiKhoanId")]
    [InverseProperty("ThanhToanTams")]
    public virtual TaiKhoan TaiKhoan { get; set; } = null!;
}
