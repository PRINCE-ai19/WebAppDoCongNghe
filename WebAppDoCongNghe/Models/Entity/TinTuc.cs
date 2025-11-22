using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebAppDoCongNghe.Models.Entity;

[Table("TinTuc")]
public partial class TinTuc
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [StringLength(255)]
    public string TieuDe { get; set; } = null!;

    public string? MoTa { get; set; }

    public string? NoiDung { get; set; }

    [StringLength(255)]
    public string? Image { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayTao { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgaySua { get; set; }

    public bool? HienThi { get; set; }
}
