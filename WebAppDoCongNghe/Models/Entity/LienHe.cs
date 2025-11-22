using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebAppDoCongNghe.Models.Entity;

[Table("LienHe")]
public partial class LienHe
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [StringLength(100)]
    public string? HoTen { get; set; }

    [StringLength(150)]
    public string? Email { get; set; }

    [StringLength(20)]
    public string? SoDienThoai { get; set; }

    public string? NoiDung { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayGui { get; set; }
}
