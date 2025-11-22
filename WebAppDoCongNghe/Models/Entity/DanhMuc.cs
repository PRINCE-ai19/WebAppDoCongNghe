using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebAppDoCongNghe.Models.Entity;

[Table("DanhMuc")]
public partial class DanhMuc
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [StringLength(100)]
    public string TenDanhMuc { get; set; } = null!;

    [StringLength(255)]
    public string? MoTa { get; set; }

    public int? ViTri { get; set; }

    [InverseProperty("DanhMuc")]
    public virtual ICollection<SanPham> SanPhams { get; set; } = new List<SanPham>();
}
