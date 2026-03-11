using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppDoCongNghe.Models.ApiRespone;
using WebAppDoCongNghe.Models.Entities;
using WebAppDoCongNghe.Models.model;
using WebAppDoCongNghe.Service;

namespace WebAppDoCongNghe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaiKhoanController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ICloudinaryService _cloudinaryService;

        public TaiKhoanController(AppDbContext context, ICloudinaryService cloudinaryService)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
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
                    hinhAnh = x.HinhAnh,
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
                message = "L?y danh sách tài kho?n thành công",
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
                    hinhAnh = x.HinhAnh,
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
                Message = "Danh sách tài kho?n",
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
                    hinhAnh = x.HinhAnh,
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
                    Message = "Không tìm th?y tài kho?n"
                });
            }

            return Ok(new ApiRespone
            {
                Success = true,
                Message = "L?y tài kho?n thành công",
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
                    Message = "Không tìm th?y tài kho?n"
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
                Message = "C?p nh?t tài kho?n thành công",
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
                    Message = "Không tìm th?y tài kho?n"
                });
            }

            _context.TaiKhoans.Remove(tk);
            await _context.SaveChangesAsync();

            return Ok(new ApiRespone
            {
                Success = true,
                Message = "Xóa tài kho?n thành công"
            });
        }

        [HttpGet("LoaiTaiKhoan")]
        public IActionResult GetAllLoaiTaiKhoan()
        {
            try
            {
                var list = _context.LoaiTaiKhoans
                    .AsNoTracking()
                    .Select(x => new
                    {
                        id = x.Id,
                        tenLoai = x.TenLoaiTaiKhoan
                    })
                    .ToList();

                return Ok(new ApiRespone
                {
                    Success = true,
                    Message = "L?y danh sách lo?i tài kho?n thành công",
                    Data = list
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiRespone
                {
                    Success = false,
                    Message = $"L?i khi l?y danh sách lo?i tài kho?n: {ex.Message}",
                    Data = null
                });
            }
        }

        [HttpPut("UpdateProfile/{id}")]
        public async Task<IActionResult> UpdateProfile(int id, [FromForm] UpdateProfileRequest model)
        {
            try
            {
                // Ki?m tra ModelState validation
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    
                    return BadRequest(new ApiRespone
                    {
                        Success = false,
                        Message = string.Join(", ", errors),
                        Data = null
                    });
                }

                var tk = await _context.TaiKhoans.FindAsync(id);

                if (tk == null)
                {
                    return NotFound(new ApiRespone
                    {
                        Success = false,
                        Message = "Không tìm th?y tài kho?n"
                    });
                }

                // C?p nh?t thông tin co b?n
                tk.HoTen = model.HoTen;
                tk.Email = model.Email;
                tk.SoDienThoai = model.SoDienThoai;
                tk.DiaChi = model.DiaChi;

                // Upload ?nh n?u có
                if (model.HinhAnh != null && model.HinhAnh.Length > 0)
                {
                    try
                    {
                        var imageUrl = await _cloudinaryService.UploadImageAsync(model.HinhAnh, "TaiKhoan");
                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            tk.HinhAnh = imageUrl;
                        }
                    }
                    catch (Exception imgEx)
                    {
                        return BadRequest(new ApiRespone
                        {
                            Success = false,
                            Message = $"L?i khi upload ?nh: {imgEx.Message}",
                            Data = null
                        });
                    }
                }

                await _context.SaveChangesAsync();

                // Tr? v? thông tin dã c?p nh?t
                var updatedTk = _context.TaiKhoans
                    .Include(x => x.IdLoaiTaiKhoanNavigation)
                    .Where(x => x.Id == id)
                    .Select(x => new
                    {
                        id = x.Id,
                        hoTen = x.HoTen,
                        email = x.Email,
                        soDienThoai = x.SoDienThoai,
                        diaChi = x.DiaChi,
                        hinhAnh = x.HinhAnh,
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

                return Ok(new ApiRespone
                {
                    Success = true,
                    Message = "C?p nh?t thông tin thành công",
                    Data = updatedTk
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiRespone
                {
                    Success = false,
                    Message = $"L?i khi c?p nh?t thông tin: {ex.Message}",
                    Data = null
                });
            }
        }

    }
}
