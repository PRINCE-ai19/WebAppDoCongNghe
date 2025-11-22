using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppDoCongNghe.Models.ApiRespone;
using WebAppDoCongNghe.Models.Entity;
using WebAppDoCongNghe.Models.model;

namespace WebAppDoCongNghe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GioHangController : ControllerBase
    {
        private readonly WebAppDoCongNgheContext _context;
        public GioHangController(  WebAppDoCongNgheContext context)
        {
            _context = context;
        }

        // lấy giỏ hàng 
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
                message = "Lấy danh sách giỏ hàng thành công",
                data = new
                {
                    items = items,
                    total = total,
                    page = page,
                    pageSize = pageSize
                }
            });
        }

        // xóa giỏ hàng admin 
        [HttpDelete("DeleteGH/{id}")]
        public IActionResult DeleteGH(int id) 
        {
            var giohang = _context.GioHangs.Find(id);
            if (giohang == null) 
            {
                return NotFound(new ApiRespone { Success = false, Message = "Không tìm thấy giỏ hàng" });
            }
            _context.GioHangs.Remove(giohang);
            _context.SaveChanges();


            return Ok(new ApiRespone { Success = true, Message = "Xóa giỏ hàng thành công" });
        }


        // . Thêm sản phẩm vào giỏ hàng
        [HttpPost("Them")]
        public IActionResult ThemVaoGioHang(GioHangBind gioHangs)
        {
            if (gioHangs == null || gioHangs.TaiKhoanId == null || gioHangs.sanPhamId == null)
            {
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            // Tìm sản phẩm
            var sanPham = _context.SanPhams.FirstOrDefault(p => p.Id == gioHangs.sanPhamId);
            if (sanPham == null)
            {
                return BadRequest(new { success = false, message = "Sản phẩm không tồn tại." });
            }

            // Kiểm tra còn hàng không
            if (sanPham.SoLuongTon == null || sanPham.SoLuongTon <= 0)
            {
                return BadRequest(new { success = false, message = "Sản phẩm đã hết hàng." });
            }

            // Tìm giỏ hàng của tài khoản
            var gioHang = _context.GioHangs
                .Include(g => g.ChiTietGioHangs)
                .FirstOrDefault(g => g.TaiKhoanId == gioHangs.TaiKhoanId);

            if (gioHang == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "User của bạn không có giỏ hàng."
                });
            }

            // Kiểm tra sản phẩm đã có trong giỏ chưa
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
                        message = $"Sản phẩm '{sanPham.TenSanPham}' chỉ còn {sanPham.SoLuongTon} cái trong kho."
                    });
                }

                chiTiet.SoLuong = soLuongMoi; // Cập nhật số lượng mới
            }
            else
            {
                if (gioHangs.SoLuong > sanPham.SoLuongTon)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"Sản phẩm '{sanPham.TenSanPham}' chỉ còn {sanPham.SoLuongTon} cái trong kho."
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
                message = "Đã thêm sản phẩm vào giỏ hàng thành công."
            });
        }


        // . Cập nhật số lượng sản phẩm
        [HttpPut("CapNhat")]
        public IActionResult CapNhatSoLuong(int chiTietId, int soLuongMoi)
        {
            if (soLuongMoi <= 0)
            {
                return BadRequest(new { success = false, message = "Số lượng phải lớn hơn 0." });
            }

            var item = _context.ChiTietGioHangs
                .Include(c => c.SanPham)
                .FirstOrDefault(c => c.Id == chiTietId);

            if (item == null)
                return NotFound(new { success = false, message = "Không tìm thấy sản phẩm trong giỏ hàng." });

            var sanPham = item.SanPham;
            if (sanPham == null)
                return NotFound(new { success = false, message = "Sản phẩm không tồn tại." });

            if (soLuongMoi > sanPham.SoLuongTon)
            {
                return BadRequest(new
                {
                    success = false,
                    message = $"Sản phẩm '{sanPham.TenSanPham}' chỉ còn {sanPham.SoLuongTon} cái trong kho."
                });
            }

            item.SoLuong = soLuongMoi;
            _context.SaveChanges();

            return Ok(new { success = true, message = "Cập nhật số lượng thành công." });
        }

        // . Xóa 1 sản phẩm khỏi giỏ hàng
        [HttpDelete("Xoa/{chiTietId}")]
        public IActionResult XoaSanPham(int chiTietId)
        {
            var item = _context.ChiTietGioHangs.Find(chiTietId);
            if (item == null)
                return NotFound(new { success = false, message = "Không tìm thấy sản phẩm trong giỏ hàng" });

            _context.ChiTietGioHangs.Remove(item);
            _context.SaveChanges();

            return Ok(new { success = true, message = "Đã xóa sản phẩm khỏi giỏ hàng" });
        }

        //. Xóa toàn bộ giỏ hàng (clear)
        [HttpDelete("Clear/{taiKhoanId}")]
        public IActionResult XoaTatCa(int taiKhoanId)
        {
            var gioHang = _context.GioHangs.AsNoTracking()
                .Include(g => g.ChiTietGioHangs)
                .FirstOrDefault(g => g.TaiKhoanId == taiKhoanId);

            if (gioHang == null)
                return NotFound(new { success = false, message = "Giỏ hàng không tồn tại" });

            _context.ChiTietGioHangs.RemoveRange(gioHang.ChiTietGioHangs);
            _context.SaveChanges();

            return Ok(new { success = true, message = "Đã xóa toàn bộ sản phẩm trong giỏ hàng" });
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
                    message = "Giỏ hàng trống.",
                    tongTien = 0,
                    soLuongSanPham = 0,
                    data = new List<object>()
                });
            }

            var data = gioHang.ChiTietGioHangs.Select(c => new
            {
                ChiTietId = c.Id,
                SanPhamId = c.SanPhamId,
                TenSanPham = c.SanPham?.TenSanPham,
                Gia = c.SanPham?.Gia ?? 0,
                SoLuong = c.SoLuong ?? 0,
                ThanhTien = (c.SanPham?.Gia ?? 0) * (c.SoLuong ?? 0),

                //  Lấy URL ảnh đầu tiên từ Cloudinary
                AnhDaiDien = c.SanPham?.HinhAnhSanPhams != null && c.SanPham.HinhAnhSanPhams.Any()
            ? c.SanPham.HinhAnhSanPhams.First().HinhAnh // hoặc .DuongDan nếu bạn đặt tên khác
            : null
            }).ToList();

            var tongTien = data.Sum(x => x.ThanhTien);
            var tongSoLuong = data.Sum(x => x.SoLuong);

            return Ok(new
            {
                success = true,
                message = "Lấy chi tiết giỏ hàng thành công.",
                tongTien,
                tongSoLuong,
                data
            });
        }
    }
}
