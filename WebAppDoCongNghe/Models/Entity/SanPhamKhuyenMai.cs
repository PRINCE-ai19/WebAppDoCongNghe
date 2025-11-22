using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebAppDoCongNghe.Models.Entity;

[Table("SanPham_KhuyenMai")]
public partial class SanPhamKhuyenMai
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("SanPhamID")]
    public int? SanPhamId { get; set; }

    [Column("KhuyenMaiID")]
    public int? KhuyenMaiId { get; set; }

    [ForeignKey("KhuyenMaiId")]
    [InverseProperty("SanPhamKhuyenMais")]
    public virtual KhuyenMai? KhuyenMai { get; set; }

    [ForeignKey("SanPhamId")]
    [InverseProperty("SanPhamKhuyenMais")]
    public virtual SanPham? SanPham { get; set; }
}
