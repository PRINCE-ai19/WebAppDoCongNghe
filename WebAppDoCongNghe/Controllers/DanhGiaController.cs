using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using WebAppDoCongNghe.Models.ApiRespone;
using WebAppDoCongNghe.Models.Entity;
using WebAppDoCongNghe.Models.model;

namespace WebAppDoCongNghe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DanhGiaController : ControllerBase
    {
        private readonly WebAppDoCongNgheContext _context;

        public DanhGiaController(WebAppDoCongNgheContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tạo đánh giá sản phẩm (chỉ khi đã mua sản phẩm)
        /// POST /api/DanhGia
        /// </summary>
        [HttpPost]
        public IActionResult CreateDanhGia([FromBody] DanhGiaRequest request, [FromQuery] int taiKhoanId)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiRespone
                    {
                        Success = false,
                        Message = "Dữ liệu không hợp lệ"
                    });
                }

                // Kiểm tra tài khoản có tồn tại không
                var taiKhoan = _context.TaiKhoans.FirstOrDefault(t => t.Id == taiKhoanId);
                if (taiKhoan == null)
                {
                    return NotFound(new ApiRespone
                    {
                        Success = false,
                        Message = "Không tìm thấy tài khoản."
                    });
                }

                // Kiểm tra sản phẩm có tồn tại không
                var sanPham = _context.SanPhams.FirstOrDefault(s => s.Id == request.SanPhamId);
                if (sanPham == null)
                {
                    return NotFound(new ApiRespone
                    {
                        Success = false,
                        Message = "Không tìm thấy sản phẩm."
                    });
                }

                // Kiểm tra user đã mua sản phẩm này chưa (thông qua ChiTietDonHang)
                var daMua = _context.DonHangs
                    .Where(d => d.TaiKhoanId == taiKhoanId && d.TrangThai != "Đã hủy")
                    .Include(d => d.ChiTietDonHangs)
                    .Any(d => d.ChiTietDonHangs.Any(ct => ct.SanPhamId == request.SanPhamId));

                if (!daMua)
                {
                    return BadRequest(new ApiRespone
                    {
                        Success = false,
                        Message = "Bạn chỉ có thể đánh giá sản phẩm đã mua."
                    });
                }

                // Kiểm tra user đã đánh giá sản phẩm này chưa
                var danhGiaCu = _context.DanhGia
                    .FirstOrDefault(d => d.TaiKhoanId == taiKhoanId && d.SanPhamId == request.SanPhamId);

                if (danhGiaCu != null)
                {
                    return BadRequest(new ApiRespone
                    {
                        Success = false,
                        Message = "Bạn đã đánh giá sản phẩm này. Vui lòng sử dụng chức năng sửa đánh giá."
                    });
                }

                // Tạo đánh giá mới
                var danhGia = new DanhGium
                {
                    TaiKhoanId = taiKhoanId,
                    SanPhamId = request.SanPhamId,
                    SoSao = request.SoSao,
                    NoiDung = request.NoiDung,
                    NgayDanhGia = DateTime.Now
                };

                _context.DanhGia.Add(danhGia);
                _context.SaveChanges();

                return Ok(new ApiRespone
                {
                    Success = true,
                    Message = "Đánh giá sản phẩm thành công.",
                    Data = new
                    {
                        danhGia.Id,
                        danhGia.TaiKhoanId,
                        danhGia.SanPhamId,
                        danhGia.SoSao,
                        danhGia.NoiDung,
                        danhGia.NgayDanhGia
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiRespone
                {
                    Success = false,
                    Message = $"Lỗi khi tạo đánh giá: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Sửa đánh giá (chỉ user tạo mới sửa được)
        /// PUT /api/DanhGia/{id}
        /// </summary>
        [HttpPut("{id}")]
        public IActionResult UpdateDanhGia(int id, [FromBody] DanhGiaRequest request, [FromQuery] int taiKhoanId)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiRespone
                    {
                        Success = false,
                        Message = "Dữ liệu không hợp lệ"
                    });
                }

                // Tìm đánh giá
                var danhGia = _context.DanhGia.FirstOrDefault(d => d.Id == id);
                if (danhGia == null)
                {
                    return NotFound(new ApiRespone
                    {
                        Success = false,
                        Message = "Không tìm thấy đánh giá."
                    });
                }

                // Kiểm tra quyền sửa (chỉ user tạo mới sửa được)
                if (danhGia.TaiKhoanId != taiKhoanId)
                {
                    return StatusCode(403, new ApiRespone
                    {
                        Success = false,
                        Message = "Bạn không có quyền sửa đánh giá này."
                    });
                }

                // Cập nhật đánh giá
                danhGia.SoSao = request.SoSao;
                danhGia.NoiDung = request.NoiDung;
                danhGia.NgayDanhGia = DateTime.Now; // Cập nhật lại ngày đánh giá

                _context.SaveChanges();

                return Ok(new ApiRespone
                {
                    Success = true,
                    Message = "Cập nhật đánh giá thành công.",
                    Data = new
                    {
                        danhGia.Id,
                        danhGia.TaiKhoanId,
                        danhGia.SanPhamId,
                        danhGia.SoSao,
                        danhGia.NoiDung,
                        danhGia.NgayDanhGia
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiRespone
                {
                    Success = false,
                    Message = $"Lỗi khi cập nhật đánh giá: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Xóa đánh giá (chỉ user tạo mới xóa được)
        /// DELETE /api/DanhGia/{id}
        /// </summary>
        [HttpDelete("{id}")]
        public IActionResult DeleteDanhGia(int id, [FromQuery] int taiKhoanId)
        {
            try
            {
                var danhGia = _context.DanhGia.FirstOrDefault(d => d.Id == id);
                if (danhGia == null)
                {
                    return NotFound(new ApiRespone
                    {
                        Success = false,
                        Message = "Không tìm thấy đánh giá."
                    });
                }

                // Kiểm tra quyền xóa (chỉ user tạo mới xóa được)
                if (danhGia.TaiKhoanId != taiKhoanId)
                {
                    return StatusCode(403, new ApiRespone
                    {
                        Success = false,
                        Message = "Bạn không có quyền xóa đánh giá này."
                    });
                }

                _context.DanhGia.Remove(danhGia);
                _context.SaveChanges();

                return Ok(new ApiRespone
                {
                    Success = true,
                    Message = "Xóa đánh giá thành công."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiRespone
                {
                    Success = false,
                    Message = $"Lỗi khi xóa đánh giá: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Lấy danh sách đánh giá theo sản phẩm (hiển thị ai đã đánh giá)
        /// GET /api/DanhGia/GetBySanPham/{sanPhamId}
        /// </summary>
        [HttpGet("GetBySanPham/{sanPhamId}")]
        public IActionResult GetBySanPham(int sanPhamId, [FromQuery] int? page = 1, [FromQuery] int? pageSize = 10)
        {
            try
            {
                // Kiểm tra sản phẩm có tồn tại không
                var sanPham = _context.SanPhams.FirstOrDefault(s => s.Id == sanPhamId);
                if (sanPham == null)
                {
                    return NotFound(new ApiRespone
                    {
                        Success = false,
                        Message = "Không tìm thấy sản phẩm."
                    });
                }

                var query = _context.DanhGia
                    .Where(d => d.SanPhamId == sanPhamId)
                    .Include(d => d.TaiKhoan)
                    .OrderByDescending(d => d.NgayDanhGia)
                    .AsQueryable();

                int total = query.Count();

                var danhGias = query
                    .Skip(((int)page - 1) * (int)pageSize)
                    .Take((int)pageSize)
                    .Select(d => new
                    {
                        d.Id,
                        d.SoSao,
                        d.NoiDung,
                        d.NgayDanhGia,
                        TaiKhoan = d.TaiKhoan != null ? new
                        {
                            d.TaiKhoan.Id,
                            d.TaiKhoan.HoTen,
                            d.TaiKhoan.Email
                        } : null
                    })
                    .ToList();

                // Tính điểm trung bình
                var diemTrungBinh = _context.DanhGia
                    .Where(d => d.SanPhamId == sanPhamId)
                    .Average(d => (double?)d.SoSao) ?? 0;

                return Ok(new ApiRespone
                {
                    Success = true,
                    Message = "Lấy danh sách đánh giá thành công.",
                    Data = new
                    {
                        items = danhGias,
                        total = total,
                        page = page,
                        pageSize = pageSize,
                        diemTrungBinh = Math.Round(diemTrungBinh, 1)
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiRespone
                {
                    Success = false,
                    Message = $"Lỗi khi lấy danh sách đánh giá: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Lấy đánh giá của user cho sản phẩm cụ thể
        /// GET /api/DanhGia/GetByUserAndSanPham?taiKhoanId={taiKhoanId}&sanPhamId={sanPhamId}
        /// </summary>
        [HttpGet("GetByUserAndSanPham")]
        public IActionResult GetByUserAndSanPham([FromQuery] int taiKhoanId, [FromQuery] int sanPhamId)
        {
            try
            {
                var danhGia = _context.DanhGia
                    .Include(d => d.TaiKhoan)
                    .FirstOrDefault(d => d.TaiKhoanId == taiKhoanId && d.SanPhamId == sanPhamId);

                if (danhGia == null)
                {
                    return Ok(new ApiRespone
                    {
                        Success = true,
                        Message = "Bạn chưa đánh giá sản phẩm này.",
                        Data = null
                    });
                }

                return Ok(new ApiRespone
                {
                    Success = true,
                    Message = "Lấy đánh giá thành công.",
                    Data = new
                    {
                        danhGia.Id,
                        danhGia.SoSao,
                        danhGia.NoiDung,
                        danhGia.NgayDanhGia,
                        TaiKhoan = danhGia.TaiKhoan != null ? new
                        {
                            danhGia.TaiKhoan.Id,
                            danhGia.TaiKhoan.HoTen,
                            danhGia.TaiKhoan.Email
                        } : null
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiRespone
                {
                    Success = false,
                    Message = $"Lỗi khi lấy đánh giá: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Lấy tất cả đánh giá của user
        /// GET /api/DanhGia/GetByUser/{taiKhoanId}
        /// </summary>
        [HttpGet("GetByUser/{taiKhoanId}")]
        public IActionResult GetByUser(int taiKhoanId)
        {
            try
            {
                var danhGias = _context.DanhGia
                    .Where(d => d.TaiKhoanId == taiKhoanId)
                    .Include(d => d.SanPham)
                        .ThenInclude(s => s.HinhAnhSanPhams)
                    .OrderByDescending(d => d.NgayDanhGia)
                    .Select(d => new
                    {
                        d.Id,
                        d.SoSao,
                        d.NoiDung,
                        d.NgayDanhGia,
                        SanPham = d.SanPham != null ? new
                        {
                            d.SanPham.Id,
                            d.SanPham.TenSanPham,
                            d.SanPham.ThuongHieu,
                            HinhAnhDaiDien = d.SanPham.HinhAnhSanPhams != null && d.SanPham.HinhAnhSanPhams.Any()
                                ? d.SanPham.HinhAnhSanPhams.First().HinhAnh
                                : null
                        } : null
                    })
                    .ToList();

                return Ok(new ApiRespone
                {
                    Success = true,
                    Message = "Lấy danh sách đánh giá thành công.",
                    Data = danhGias
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiRespone
                {
                    Success = false,
                    Message = $"Lỗi khi lấy danh sách đánh giá: {ex.Message}"
                });
            }
        }
    }
}

