using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebAppDoCongNghe.Models.Entity;

[Table("ThongBao")]
public partial class ThongBao
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("TaiKhoanID")]
    public int TaiKhoanId { get; set; }

    [StringLength(255)]
    public string? TieuDe { get; set; }

    public string? NoiDung { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayTao { get; set; }

    public bool? DaXem { get; set; }

    [ForeignKey("TaiKhoanId")]
    [InverseProperty("ThongBaos")]
    public virtual TaiKhoan TaiKhoan { get; set; } = null!;
}
