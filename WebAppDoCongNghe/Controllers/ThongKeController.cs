using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppDoCongNghe.Models.ApiRespone;
using WebAppDoCongNghe.Models.Entities;

namespace WebAppDoCongNghe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ThongKeController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ThongKeController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("DoanhThuTheoThang")]
        public async Task<IActionResult> GetDoanhThuTheoThang()
        {
            try
            {
                var year = DateTime.Now.Year;
                var data = await _context.DonHangs
                    .Where(d => d.NgayDat.HasValue && d.NgayDat.Value.Year == year && d.TrangThai != "Đã hủy")
                    .GroupBy(d => d.NgayDat.Value.Month)
                    .Select(g => new
                    {
                        Thang = g.Key,
                        DoanhThu = g.Sum(d => d.TongTien ?? 0)
                    })
                    .OrderBy(x => x.Thang)
                    .ToListAsync();

                // Fill missing months
                var result = Enumerable.Range(1, 12).Select(month => new
                {
                    Thang = $"Tháng {month}",
                    DoanhThu = data.FirstOrDefault(d => d.Thang == month)?.DoanhThu ?? 0
                }).ToList();

                return Ok(new ApiRespone
                {
                    Success = true,
                    Message = "Lấy thống kê doanh thu theo tháng thành công.",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiRespone
                {
                    Success = false,
                    Message = $"Lỗi khi lấy thống kê doanh thu: {ex.Message}"
                });
            }
        }

        [HttpGet("TopFavoriteProducts")]
        public async Task<IActionResult> GetTopFavoriteProducts()
        {
            try
            {
                var topFavorites = await _context.SanPhamYeuThiches
                    .GroupBy(y => y.SanPhamId)
                    .Select(g => new
                    {
                        SanPhamId = g.Key,
                        SoLuongYeuThich = g.Count()
                    })
                    .OrderByDescending(x => x.SoLuongYeuThich)
                    .Take(5)
                    .ToListAsync();

                var productIds = topFavorites.Select(f => f.SanPhamId).ToList();
                var products = await _context.SanPhams
                    .Where(p => productIds.Contains(p.Id))
                    .Include(p => p.HinhAnhSanPhams)
                    .ToListAsync();

                var result = topFavorites.Select(f => {
                    var product = products.FirstOrDefault(p => p.Id == f.SanPhamId);
                    return new
                    {
                        f.SanPhamId,
                        TenSanPham = product?.TenSanPham ?? "Sản phẩm không tồn tại",
                        HinhAnh = product?.HinhAnhSanPhams?.FirstOrDefault()?.HinhAnh,
                        SoLuongYeuThich = f.SoLuongYeuThich
                    };
                }).ToList();

                return Ok(new ApiRespone
                {
                    Success = true,
                    Message = "Lấy top sản phẩm yêu thích thành công.",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiRespone
                {
                    Success = false,
                    Message = $"Lỗi khi lấy top yêu thích: {ex.Message}"
                });
            }
        }

        [HttpGet("BestSellingProducts")]
        public async Task<IActionResult> GetBestSellingProducts()
        {
            try
            {
                var bestSellers = await _context.ChiTietDonHangs
                    .Include(ct => ct.DonHang)
                    .Where(ct => ct.DonHang != null && ct.DonHang.TrangThai != "Đã hủy")
                    .GroupBy(ct => ct.SanPhamId)
                    .Select(g => new
                    {
                        SanPhamId = g.Key,
                        SoLuongDaBan = g.Sum(ct => ct.SoLuong ?? 0)
                    })
                    .OrderByDescending(x => x.SoLuongDaBan)
                    .Take(5)
                    .ToListAsync();

                var productIds = bestSellers.Select(b => b.SanPhamId).ToList();
                var products = await _context.SanPhams
                    .Where(p => productIds.Contains(p.Id))
                    .Include(p => p.HinhAnhSanPhams)
                    .ToListAsync();

                var result = bestSellers.Select(b => {
                    var product = products.FirstOrDefault(p => p.Id == b.SanPhamId);
                    return new
                    {
                        b.SanPhamId,
                        TenSanPham = product?.TenSanPham ?? "Sản phẩm không tồn tại",
                        HinhAnh = product?.HinhAnhSanPhams?.FirstOrDefault()?.HinhAnh,
                        SoLuongDaBan = b.SoLuongDaBan,
                        Gia = product?.Gia ?? 0
                    };
                }).ToList();

                return Ok(new ApiRespone
                {
                    Success = true,
                    Message = "Lấy top sản phẩm bán chạy thành công.",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiRespone
                {
                    Success = false,
                    Message = $"Lỗi khi lấy top bán chạy: {ex.Message}"
                });
            }
        }
    }
}
