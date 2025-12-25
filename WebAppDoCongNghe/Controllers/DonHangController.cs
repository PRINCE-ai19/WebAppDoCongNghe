using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using WebAppDoCongNghe.Models.ApiRespone;
using WebAppDoCongNghe.Models.Entity;
using WebAppDoCongNghe.Models.model;

namespace WebAppDoCongNghe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DonHangController : ControllerBase
    {
        private readonly WebAppDoCongNgheContext _context;

        public DonHangController(WebAppDoCongNgheContext context)
        {
            _context = context;
        }

     
        [HttpGet("GetByTaiKhoan/{taiKhoanId}")]
        public IActionResult GetByTaiKhoan(int taiKhoanId)
        {
            try
            {
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

                // Lấy danh sách đơn hàng của tài khoản - Load data trước rồi mới select
                var donHangsList = _context.DonHangs
                    .Where(d => d.TaiKhoanId == taiKhoanId)
                    .Include(d => d.ChiTietDonHangs)
                        .ThenInclude(ct => ct.SanPham)
                            .ThenInclude(sp => sp.HinhAnhSanPhams)
                    .Include(d => d.PhieuGiamGia)
                    .OrderByDescending(d => d.NgayDat)
                    .ToList();

                if (!donHangsList.Any())
                {
                    return Ok(new ApiRespone
                    {
                        Success = true,
                        Message = "Bạn chưa có đơn hàng nào.",
                        Data = new List<object>()
                    });
                }

                // Map sang DTO để tránh object cycle
                var donHangs = donHangsList.Select(MapDonHangListDto).ToList();

                return Ok(new ApiRespone
                {
                    Success = true,
                    Message = "Lấy danh sách đơn hàng thành công.",
                    Data = donHangs
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiRespone
                {
                    Success = false,
                    Message = $"Lỗi khi lấy đơn hàng: {ex.Message}"
                });
            }
        }

     
        [HttpGet("admin")]
        public IActionResult GetAll([FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 20;

            try
            {
                var query = _context.DonHangs
                    .Include(d => d.ChiTietDonHangs)
                        .ThenInclude(ct => ct.SanPham)
                            .ThenInclude(sp => sp.HinhAnhSanPhams)
                    .Include(d => d.PhieuGiamGia)
                    .Include(d => d.TaiKhoan)
                    .OrderByDescending(d => d.NgayDat)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(status))
                {
                    query = query.Where(d => d.TrangThai == status);
                }

                var total = query.Count();

                var items = query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList()
                    .Select(MapDonHangListDto)
                    .ToList();

                return Ok(new ApiRespone
                {
                    Success = true,
                    Message = "Lấy danh sách đơn hàng thành công.",
                    Data = new
                    {
                        page,
                        pageSize,
                        total,
                        items
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiRespone
                {
                    Success = false,
                    Message = $"Lỗi khi lấy danh sách đơn hàng: {ex.Message}"
                });
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            try
            {
                var donHang = _context.DonHangs
                    .Include(d => d.ChiTietDonHangs)
                        .ThenInclude(ct => ct.SanPham)
                            .ThenInclude(sp => sp.HinhAnhSanPhams)
                    .Include(d => d.PhieuGiamGia)
                    .Include(d => d.TaiKhoan)
                    .FirstOrDefault(d => d.Id == id);

                if (donHang == null)
                {
                    return NotFound(new ApiRespone
                    {
                        Success = false,
                        Message = "Không tìm thấy đơn hàng."
                    });
                }

                var result = new
                {
                    donHang.Id,
                    donHang.NgayDat,
                    donHang.TongTien,
                    donHang.TrangThai,
                    donHang.DiaChiGiao,
                    donHang.GhiChu,
                    donHang.PhuongThucThanhToan,
                    TaiKhoan = donHang.TaiKhoan != null ? new
                    {
                        donHang.TaiKhoan.Id,
                        donHang.TaiKhoan.HoTen,
                        donHang.TaiKhoan.Email
                    } : null,
                    PhieuGiamGia = donHang.PhieuGiamGia != null ? new
                    {
                        donHang.PhieuGiamGia.Id,
                        donHang.PhieuGiamGia.MaPhieu,
                        donHang.PhieuGiamGia.MoTa
                    } : null,
                    ChiTietDonHangs = donHang.ChiTietDonHangs.Select(ct => new
                    {
                        ct.Id,
                        ct.SanPhamId,
                        ct.SoLuong,
                        ct.DonGia,
                        SanPham = ct.SanPham != null ? new
                        {
                            ct.SanPham.Id,
                            ct.SanPham.TenSanPham,
                            ct.SanPham.ThuongHieu,
                            ct.SanPham.Gia,
                            ct.SanPham.GiaGiam,
                            ct.SanPham.MoTa,
                            HinhAnhDaiDien = ct.SanPham.HinhAnhSanPhams != null && ct.SanPham.HinhAnhSanPhams.Any()
                                ? ct.SanPham.HinhAnhSanPhams.First().HinhAnh
                                : null,
                            HinhAnh = ct.SanPham.HinhAnhSanPhams != null && ct.SanPham.HinhAnhSanPhams.Any()
                                ? ct.SanPham.HinhAnhSanPhams.Select(h => new
                                {
                                    h.Id,
                                    h.HinhAnh
                                }).Cast<object>().ToList()
                                : new List<object>(),
                            ThanhTien = (ct.DonGia ?? 0) * (ct.SoLuong ?? 0)
                        } : null
                    }).ToList()
                };

                return Ok(new ApiRespone
                {
                    Success = true,
                    Message = "Lấy chi tiết đơn hàng thành công.",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiRespone
                {
                    Success = false,
                    Message = $"Lỗi khi lấy chi tiết đơn hàng: {ex.Message}"
                });
            }
        }

        [HttpPatch("{id}/status")]
        public IActionResult UpdateStatus(int id, [FromBody] UpdateOrderStatusRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.TrangThaiMoi))
            {
                return BadRequest(new ApiRespone
                {
                    Success = false,
                    Message = "Trạng thái mới không hợp lệ."
                });
            }

            try
            {
                var donHang = _context.DonHangs.FirstOrDefault(d => d.Id == id);
                if (donHang == null)
                {
                    return NotFound(new ApiRespone
                    {
                        Success = false,
                        Message = "Không tìm thấy đơn hàng."
                    });
                }

                if (!OrderStatus.CanTransit(donHang.TrangThai, request.TrangThaiMoi))
                {
                    return BadRequest(new ApiRespone
                    {
                        Success = false,
                        Message = $"Không thể chuyển từ trạng thái \"{donHang.TrangThai}\" sang \"{request.TrangThaiMoi}\"."
                    });
                }

                donHang.TrangThai = request.TrangThaiMoi;
                if (!string.IsNullOrWhiteSpace(request.GhiChu))
                {
                    donHang.GhiChu = string.IsNullOrWhiteSpace(donHang.GhiChu)
                        ? request.GhiChu
                        : $"{donHang.GhiChu}\n{request.GhiChu}";
                }

                _context.SaveChanges();

                return Ok(new ApiRespone
                {
                    Success = true,
                    Message = "Cập nhật trạng thái đơn hàng thành công."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiRespone
                {
                    Success = false,
                    Message = $"Lỗi khi cập nhật trạng thái đơn hàng: {ex.Message}"
                });
            }
        }

        [HttpPost("{id}/cancel")]
        public IActionResult CancelOrder(int id, [FromBody] CancelOrderRequest? request)
        {
            try
            {
                var donHang = _context.DonHangs.FirstOrDefault(d => d.Id == id);
                if (donHang == null)
                {
                    return NotFound(new ApiRespone
                    {
                        Success = false,
                        Message = "Không tìm thấy đơn hàng."
                    });
                }

                if (!OrderStatus.CanCancel(donHang.TrangThai))
                {
                    return BadRequest(new ApiRespone
                    {
                        Success = false,
                        Message = "Chỉ có thể hủy đơn khi đang ở trạng thái 'Chờ xử lý' hoặc 'Đang chuẩn bị hàng'."
                    });
                }

                donHang.TrangThai = OrderStatus.DaHuy;
                if (!string.IsNullOrWhiteSpace(request?.LyDo))
                {
                    donHang.GhiChu = string.IsNullOrWhiteSpace(donHang.GhiChu)
                        ? $"[Hủy đơn] {request.LyDo}"
                        : $"{donHang.GhiChu}\n[Hủy đơn] {request.LyDo}";
                }

                _context.SaveChanges();

                return Ok(new ApiRespone
                {
                    Success = true,
                    Message = "Hủy đơn hàng thành công."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiRespone
                {
                    Success = false,
                    Message = $"Lỗi khi hủy đơn hàng: {ex.Message}"
                });
            }
        }

        private static object MapDonHangListDto(DonHang d)
        {
            return new
            {
                d.Id,
                d.NgayDat,
                d.TongTien,
                d.TrangThai,
                d.DiaChiGiao,
                d.GhiChu,
                d.PhuongThucThanhToan,
                PhieuGiamGia = d.PhieuGiamGia != null ? new
                {
                    d.PhieuGiamGia.Id,
                    d.PhieuGiamGia.MaPhieu,
                    d.PhieuGiamGia.MoTa
                } : null,
                ChiTietDonHangs = d.ChiTietDonHangs.Select(ct => new
                {
                    ct.Id,
                    ct.SanPhamId,
                    ct.SoLuong,
                    ct.DonGia,
                    SanPham = ct.SanPham != null ? new
                    {
                        ct.SanPham.Id,
                        ct.SanPham.TenSanPham,
                        ct.SanPham.ThuongHieu,
                        ct.SanPham.Gia,
                        ct.SanPham.GiaGiam,
                        HinhAnhDaiDien = ct.SanPham.HinhAnhSanPhams != null && ct.SanPham.HinhAnhSanPhams.Any()
                            ? ct.SanPham.HinhAnhSanPhams.First().HinhAnh
                            : null,
                        ThanhTien = (ct.DonGia ?? 0) * (ct.SoLuong ?? 0)
                    } : null
                }).ToList()
            };
        }
    }
}

