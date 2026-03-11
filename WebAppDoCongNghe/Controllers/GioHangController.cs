using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppDoCongNghe.Models.ApiRespone;
using WebAppDoCongNghe.Models.Entities;
using WebAppDoCongNghe.Models.model;

namespace WebAppDoCongNghe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GioHangController : ControllerBase
    {
        private readonly AppDbContext _context;
        public GioHangController(  AppDbContext context)
        {
            _context = context;
        }

        // Helper method d? tính giá gi?m t? khuy?n mãi
        private decimal TinhGiaGiamTuKhuyenMai(decimal giaGoc, int sanPhamId)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            
            // L?y khuy?n mãi dang active cho s?n ph?m này
            var khuyenMaiActive = _context.SanPhamKhuyenMais
                .Include(spkm => spkm.KhuyenMai)
                .Where(spkm => spkm.SanPhamId == sanPhamId 
                    && spkm.KhuyenMai != null
                    && spkm.KhuyenMai.NgayBatDau <= today 
                    && spkm.KhuyenMai.NgayKetThuc >= today
                    && spkm.KhuyenMai.PhanTramGiam.HasValue)
                .Select(spkm => spkm.KhuyenMai.PhanTramGiam.Value)
                .OrderByDescending(pt => pt)
                .FirstOrDefault();

            if (khuyenMaiActive > 0)
            {
                // Tính giá gi?m: giá g?c * (1 - ph?n tram gi?m / 100)
                return giaGoc * (1 - khuyenMaiActive / 100);
            }

            // N?u không có khuy?n mãi, tr? v? giá g?c
            return giaGoc;
        }

        // l?y gi? hàng 
        [HttpGet("paging")]
        public IActionResult GetPagingGH(int page, int pageSize)
        {
            var query = _context.GioHangs
                .Include(g => g.TaiKhoan)
                .AsQueryable();

            int total = query.Count();

            var items = query
                     .OrderByDescending(x => x.Id)
                     .Skip((page - 1) * pageSize)
                     .Take(pageSize)
                     .Select(g => new
                     {
                         Id = g.Id,
                         TaiKhoanId = g.TaiKhoanId,
                         NgayTao = g.NgayTao,
                         TaiKhoan = g.TaiKhoan != null ? new
                         {
                             Id = g.TaiKhoan.Id,
                             HoTen = g.TaiKhoan.HoTen,
                             Email = g.TaiKhoan.Email
                         } : null
                     })
                     .ToList();

            return Ok(new
            {
                success = true,
                message = "L?y danh sách gi? hàng thành công",
                data = new
                {
                    items = items,
                    total = total,
                    page = page,
                    pageSize = pageSize
                }
            });
        }

        // xóa gi? hàng admin 
        [HttpDelete("DeleteGH/{id}")]
        public IActionResult DeleteGH(int id) 
        {
            var giohang = _context.GioHangs.Find(id);
            if (giohang == null) 
            {
                return NotFound(new ApiRespone { Success = false, Message = "Không tìm th?y gi? hàng" });
            }
            _context.GioHangs.Remove(giohang);
            _context.SaveChanges();


            return Ok(new ApiRespone { Success = true, Message = "Xóa gi? hàng thành công" });
        }


        // . Thêm s?n ph?m vào gi? hàng
        [HttpPost("Them")]
        public IActionResult ThemVaoGioHang(GioHangBind gioHangs)
        {
            if (gioHangs == null || gioHangs.TaiKhoanId == null || gioHangs.sanPhamId == null)
            {
                return BadRequest(new { success = false, message = "D? li?u không h?p l?." });
            }

            // Tìm s?n ph?m
            var sanPham = _context.SanPhams.FirstOrDefault(p => p.Id == gioHangs.sanPhamId);
            if (sanPham == null)
            {
                return BadRequest(new { success = false, message = "S?n ph?m không t?n t?i." });
            }

            // Ki?m tra còn hàng không
            if (sanPham.SoLuongTon == null || sanPham.SoLuongTon <= 0)
            {
                return BadRequest(new { success = false, message = "S?n ph?m dã h?t hàng." });
            }

            // Tìm gi? hàng c?a tài kho?n
            var gioHang = _context.GioHangs
                .Include(g => g.ChiTietGioHangs)
                .FirstOrDefault(g => g.TaiKhoanId == gioHangs.TaiKhoanId);

            if (gioHang == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "User c?a b?n không có gi? hàng."
                });
            }

            // Ki?m tra s?n ph?m dã có trong gi? chua
            var chiTiet = gioHang.ChiTietGioHangs
                .FirstOrDefault(c => c.SanPhamId == gioHangs.sanPhamId);

            if (chiTiet != null)
            {
                var soLuongMoi = chiTiet.SoLuong + gioHangs.SoLuong;

                if (soLuongMoi > sanPham.SoLuongTon)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"S?n ph?m '{sanPham.TenSanPham}' ch? còn {sanPham.SoLuongTon} cái trong kho."
                    });
                }

                chiTiet.SoLuong = soLuongMoi; // C?p nh?t s? lu?ng m?i
            }
            else
            {
                if (gioHangs.SoLuong > sanPham.SoLuongTon)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"S?n ph?m '{sanPham.TenSanPham}' ch? còn {sanPham.SoLuongTon} cái trong kho."
                    });
                }

                var newItem = new ChiTietGioHang
                {
                    GioHangId = gioHang.Id,
                    SanPhamId = gioHangs.sanPhamId,
                    SoLuong = gioHangs.SoLuong
                };
                _context.ChiTietGioHangs.Add(newItem);
            }

            _context.SaveChanges();

            return Ok(new
            {
                success = true,
                message = "Ðã thêm s?n ph?m vào gi? hàng thành công."
            });
        }


        // . C?p nh?t s? lu?ng s?n ph?m
        [HttpPut("CapNhat")]
        public IActionResult CapNhatSoLuong(int chiTietId, int soLuongMoi)
        {
            if (soLuongMoi <= 0)
            {
                return BadRequest(new { success = false, message = "S? lu?ng ph?i l?n hon 0." });
            }

            var item = _context.ChiTietGioHangs
                .Include(c => c.SanPham)
                .FirstOrDefault(c => c.Id == chiTietId);

            if (item == null)
                return NotFound(new { success = false, message = "Không tìm th?y s?n ph?m trong gi? hàng." });

            var sanPham = item.SanPham;
            if (sanPham == null)
                return NotFound(new { success = false, message = "S?n ph?m không t?n t?i." });

            if (soLuongMoi > sanPham.SoLuongTon)
            {
                return BadRequest(new
                {
                    success = false,
                    message = $"S?n ph?m '{sanPham.TenSanPham}' ch? còn {sanPham.SoLuongTon} cái trong kho."
                });
            }

            item.SoLuong = soLuongMoi;
            _context.SaveChanges();

            return Ok(new { success = true, message = "C?p nh?t s? lu?ng thành công." });
        }

        // Xóa 1 s?n ph?m kh?i gi? hàng
        [HttpDelete("Xoa/{chiTietId}")]
        public IActionResult XoaSanPham(int chiTietId)
        {
            var item = _context.ChiTietGioHangs.Find(chiTietId);
            if (item == null)
                return NotFound(new { success = false, message = "Không tìm th?y s?n ph?m trong gi? hàng" });

            _context.ChiTietGioHangs.Remove(item);
            _context.SaveChanges();

            return Ok(new { success = true, message = "Ðã xóa s?n ph?m kh?i gi? hàng" });
        }

        // Xóa toàn b? gi? hàng (clear)
        [HttpDelete("Clear/{taiKhoanId}")]
        public IActionResult XoaTatCa(int taiKhoanId)
        {
            var gioHang = _context.GioHangs.AsNoTracking()
                .Include(g => g.ChiTietGioHangs)
                .FirstOrDefault(g => g.TaiKhoanId == taiKhoanId);

            if (gioHang == null)
                return NotFound(new { success = false, message = "Gi? hàng không t?n t?i" });

            _context.ChiTietGioHangs.RemoveRange(gioHang.ChiTietGioHangs);
            _context.SaveChanges();

            return Ok(new { success = true, message = "Ðã xóa toàn b? s?n ph?m trong gi? hàng" });
        }

        [HttpGet("XemChiTiet/{taiKhoanId}")]
        public IActionResult XemChiTietGioHang(int taiKhoanId)
        {
            var gioHang = _context.GioHangs
                          .Include(g => g.ChiTietGioHangs)
                          .ThenInclude(c => c.SanPham)
                          .ThenInclude(s => s.HinhAnhSanPhams)
                          .FirstOrDefault(g => g.TaiKhoanId == taiKhoanId);

            if (gioHang == null || gioHang.ChiTietGioHangs == null || gioHang.ChiTietGioHangs.Count == 0)
            {
                return Ok(new
                {
                    success = true,
                    message = "Gi? hàng tr?ng.",
                    tongTien = 0,
                    soLuongSanPham = 0,
                    data = new List<object>()
                });
            }

            var today = DateOnly.FromDateTime(DateTime.Now);
            var data = gioHang.ChiTietGioHangs.Select(c => {
                var giaGoc = c.SanPham?.Gia ?? 0;
                var sanPhamId = c.SanPhamId ?? 0;
                
                // Tính giá gi?m t? khuy?n mãi
                var giaGiamTuKhuyenMai = TinhGiaGiamTuKhuyenMai(giaGoc, sanPhamId);
                
                // Uu tiên giá gi?m t? khuy?n mãi, n?u không có thì dùng giá gi?m cu ho?c giá g?c
                var giaCuoiCung = giaGiamTuKhuyenMai < giaGoc ? giaGiamTuKhuyenMai : (c.SanPham?.GiaGiam ?? giaGoc);
                var soLuong = c.SoLuong ?? 0;
                var thanhTien = giaCuoiCung * soLuong;

                return new
                {
                    ChiTietId = c.Id,
                    SanPhamId = c.SanPhamId,
                    TenSanPham = c.SanPham?.TenSanPham,
                    Gia = giaGoc,
                    GiaGiam = giaCuoiCung,
                    SoLuong = soLuong,
                    ThanhTien = thanhTien,

                    //  L?y URL ?nh d?u tiên t? Cloudinary
                    AnhDaiDien = c.SanPham?.HinhAnhSanPhams != null && c.SanPham.HinhAnhSanPhams.Any()
                ? c.SanPham.HinhAnhSanPhams.First().HinhAnh // ho?c .DuongDan n?u b?n d?t tên khác
                : null
                };
            }).ToList();

            var tongTien = data.Sum(x => x.ThanhTien);
            var tongSoLuong = data.Sum(x => x.SoLuong);

            return Ok(new
            {
                success = true,
                message = "L?y chi ti?t gi? hàng thành công.",
                tongTien,
                tongSoLuong,
                data
            });
        }
    }
}
