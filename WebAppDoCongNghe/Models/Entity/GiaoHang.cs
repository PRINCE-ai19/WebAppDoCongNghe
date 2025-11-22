using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebAppDoCongNghe.Models.Entity;

[Table("GiaoHang")]
public partial class GiaoHang
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("DonHangID")]
    public int? DonHangId { get; set; }

    [StringLength(100)]
    public string? TrangThai { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayCapNhat { get; set; }

    [StringLength(100)]
    public string? DonViVanChuyen { get; set; }

    [ForeignKey("DonHangId")]
    [InverseProperty("GiaoHangs")]
    public virtual DonHang? DonHang { get; set; }
}
