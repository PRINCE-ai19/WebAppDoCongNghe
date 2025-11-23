using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebAppDoCongNghe.Models.Entity;

[Table("TaiKhoan_PhanQuyen")]
public partial class TaiKhoanPhanQuyen
{
    [Key]
    public int Id { get; set; }

    public int IdChucNang { get; set; }

    public bool? QuyenXem { get; set; }

    public bool? QuyenThem { get; set; }

    public bool? QuyenSua { get; set; }

    public bool? QuyenXoa { get; set; }

    public int IdTaiKhoan { get; set; }

    [ForeignKey("IdChucNang")]
    [InverseProperty("TaiKhoanPhanQuyens")]
    public virtual PhanQuyen IdChucNangNavigation { get; set; } = null!;

    [ForeignKey("IdTaiKhoan")]
    [InverseProperty("TaiKhoanPhanQuyens")]
    public virtual TaiKhoan IdTaiKhoanNavigation { get; set; } = null!;
}
