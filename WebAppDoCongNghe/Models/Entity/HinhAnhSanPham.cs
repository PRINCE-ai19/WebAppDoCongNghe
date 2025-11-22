using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebAppDoCongNghe.Models.Entity;

[Table("HinhAnhSanPham")]
public partial class HinhAnhSanPham
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("SanPhamID")]
    public int SanPhamId { get; set; }

    public string? HinhAnh { get; set; }

    [ForeignKey("SanPhamId")]
    [InverseProperty("HinhAnhSanPhams")]
    public virtual SanPham SanPham { get; set; } = null!;
}
