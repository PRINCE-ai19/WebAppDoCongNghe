using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebAppDoCongNghe.Models.Entity;

[Table("KhuyenMai")]
public partial class KhuyenMai
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [StringLength(100)]
    public string? TenKhuyenMai { get; set; }

    [StringLength(255)]
    public string? MoTa { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? PhanTramGiam { get; set; }

    public DateOnly? NgayBatDau { get; set; }

    public DateOnly? NgayKetThuc { get; set; }

    [InverseProperty("KhuyenMai")]
    public virtual ICollection<SanPhamKhuyenMai> SanPhamKhuyenMais { get; set; } = new List<SanPhamKhuyenMai>();
}
