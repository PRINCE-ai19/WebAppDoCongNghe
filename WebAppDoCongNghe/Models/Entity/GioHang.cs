using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebAppDoCongNghe.Models.Entity;

[Table("GioHang")]
public partial class GioHang
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("TaiKhoanID")]
    public int? TaiKhoanId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayTao { get; set; }

    [InverseProperty("GioHang")]
    public virtual ICollection<ChiTietGioHang> ChiTietGioHangs { get; set; } = new List<ChiTietGioHang>();

    [ForeignKey("TaiKhoanId")]
    [InverseProperty("GioHangs")]
    public virtual TaiKhoan? TaiKhoan { get; set; }
}
