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
    public class TaiKhoanController : ControllerBase
    {
        private readonly WebAppDoCongNgheContext _context;

        public TaiKhoanController(WebAppDoCongNgheContext context)
        {
            _context = context;
        }

        [HttpGet("paging")]
        public IActionResult GetPagingTK(int page, int pageSize)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var query = _context.TaiKhoans
                .Include(x => x.IdLoaiTaiKhoanNavigation)
                .AsNoTracking()
                .Select(x => new
                {
                    id = x.Id,
                    hoTen = x.HoTen,
                    email = x.Email,
                    soDienThoai = x.SoDienThoai,
                    diaChi = x.DiaChi,
                    ngayTao = x.NgayDangKy,
                    idLoaiTaiKhoan = x.IdLoaiTaiKhoan,
                    idLoaiTaiKhoanNavigation = x.IdLoaiTaiKhoanNavigation == null
                        ? null
                        : new
                        {
                            id = x.IdLoaiTaiKhoanNavigation.Id,
                            tenLoai = x.IdLoaiTaiKhoanNavigation.TenLoaiTaiKhoan
                        }
                });

            int total = query.Count();

            var items = query
                .OrderByDescending(x => x.id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(new
            {
                success = true,
                message = "Lấy danh sách tài khoản thành công",
                data = new
                {
                    items,
                    total,
                    page,
                    pageSize
                }
            });
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var list = _context.TaiKhoans
                .Include(x => x.IdLoaiTaiKhoanNavigation)
                .AsNoTracking()
                .Select(x => new
                {
                    id = x.Id,
                    hoTen = x.HoTen,
                    email = x.Email,
                    soDienThoai = x.SoDienThoai,
                    diaChi = x.DiaChi,
                    ngayTao = x.NgayDangKy,
                    idLoaiTaiKhoan = x.IdLoaiTaiKhoan,
                    idLoaiTaiKhoanNavigation = x.IdLoaiTaiKhoanNavigation == null
                        ? null
                        : new
                        {
                            id = x.IdLoaiTaiKhoanNavigation.Id,
                            tenLoai = x.IdLoaiTaiKhoanNavigation.TenLoaiTaiKhoan
                        }
                })
                .ToList();

            return Ok(new ApiRespone
            {
                Success = true,
                Message = "Danh sách tài khoản",
                Data = list
            });
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var tk = _context.TaiKhoans
                .Include(x => x.IdLoaiTaiKhoanNavigation)
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    id = x.Id,
                    hoTen = x.HoTen,
                    email = x.Email,
                    soDienThoai = x.SoDienThoai,
                    diaChi = x.DiaChi,
                    ngayTao = x.NgayDangKy,
                    idLoaiTaiKhoan = x.IdLoaiTaiKhoan,
                    idLoaiTaiKhoanNavigation = x.IdLoaiTaiKhoanNavigation == null
                        ? null
                        : new
                        {
                            id = x.IdLoaiTaiKhoanNavigation.Id,
                            tenLoai = x.IdLoaiTaiKhoanNavigation.TenLoaiTaiKhoan
                        }
                })
                .FirstOrDefault();

            if (tk == null)
            {
                return NotFound(new ApiRespone
                {
                    Success = false,
                    Message = "Không tìm thấy tài khoản"
                });
            }

            return Ok(new ApiRespone
            {
                Success = true,
                Message = "Lấy tài khoản thành công",
                Data = tk
            });
        }

       
        [HttpPut("Sua/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TaiKhoanRequest model)
        {
            var tk = await _context.TaiKhoans.FindAsync(id);

            if (tk == null)
            {
                return NotFound(new ApiRespone
                {
                    Success = false,
                    Message = "Không tìm thấy tài khoản"
                });
            }

            tk.HoTen = model.HoTen;
            tk.Email = model.Email;
            tk.SoDienThoai = model.SoDienThoai;
            tk.DiaChi = model.DiaChi;
            tk.IdLoaiTaiKhoan = model.IdLoaiTaiKhoan;

            await _context.SaveChangesAsync();

            return Ok(new ApiRespone
            {
                Success = true,
                Message = "Cập nhật tài khoản thành công",
                Data = tk
            });
        }

      
        [HttpDelete("Xoa/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var tk = await _context.TaiKhoans.FindAsync(id);

            if (tk == null)
            {
                return NotFound(new ApiRespone
                {
                    Success = false,
                    Message = "Không tìm thấy tài khoản"
                });
            }

            _context.TaiKhoans.Remove(tk);
            await _context.SaveChangesAsync();

            return Ok(new ApiRespone
            {
                Success = true,
                Message = "Xóa tài khoản thành công"
            });
        }

    }
}
