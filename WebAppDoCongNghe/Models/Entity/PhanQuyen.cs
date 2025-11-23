using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebAppDoCongNghe.Models.Entity;

[Table("PhanQuyen")]
public partial class PhanQuyen
{
    [Key]
    public int Id { get; set; }

    [StringLength(100)]
    public string? MaChucNang { get; set; }

    [StringLength(255)]
    public string? MoTa { get; set; }

    [InverseProperty("IdChucNangNavigation")]
    public virtual ICollection<TaiKhoanPhanQuyen> TaiKhoanPhanQuyens { get; set; } = new List<TaiKhoanPhanQuyen>();
}
