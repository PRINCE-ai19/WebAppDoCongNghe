using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using WebAppDoCongNghe.Models.ApiRespone;
using WebAppDoCongNghe.Models.Entity;

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
                var donHangs = donHangsList.Select(d => new
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
                }).ToList();

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
    }
}

