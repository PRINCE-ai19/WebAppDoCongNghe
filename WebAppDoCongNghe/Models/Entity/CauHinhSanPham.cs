using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebAppDoCongNghe.Models.Entity;

[Table("CauHinhSanPham")]
public partial class CauHinhSanPham
{
    [Key]
    public int Id { get; set; }

    public int SanPhamId { get; set; }

    [StringLength(255)]
    public string? TenThongSo { get; set; }

    [StringLength(255)]
    public string? GiaTri { get; set; }

    [ForeignKey("SanPhamId")]
    [InverseProperty("CauHinhSanPhams")]
    public virtual SanPham SanPham { get; set; } = null!;
}
