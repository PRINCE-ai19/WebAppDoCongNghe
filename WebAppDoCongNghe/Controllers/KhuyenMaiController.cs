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
    public class KhuyenMaiController : ControllerBase
    {
        private readonly WebAppDoCongNgheContext _context;

        public KhuyenMaiController(WebAppDoCongNgheContext context)
        {
            _context = context;
        }

        // Lấy danh sách khuyến mãi có phân trang
        [HttpGet("paging")]
        public IActionResult GetPaging(int page, int pageSize)
        {
            var query = _context.KhuyenMais
                .Include(k => k.SanPhamKhuyenMais)
                    .ThenInclude(spkm => spkm.SanPham)
                .Select(k => new
                {
                    k.Id,
                    k.TenKhuyenMai,
                    k.MoTa,
                    k.PhanTramGiam,
                    k.NgayBatDau,
                    k.NgayKetThuc,
                    SoSanPham = k.SanPhamKhuyenMais.Count,
                    TrangThai = k.NgayKetThuc < DateOnly.FromDateTime(DateTime.Now) ? "Hết hạn" :
                               k.NgayBatDau > DateOnly.FromDateTime(DateTime.Now) ? "Sắp diễn ra" : "Đang diễn ra"
                })
                .ToList()
                .AsQueryable();

            int total = query.Count();

            var items = query
                .OrderByDescending(x => x.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(new
            {
                success = true,
                message = "Lấy danh sách khuyến mãi thành công",
                data = new
                {
                    items = items,
                    total = total,
                    page = page,
                    pageSize = pageSize
                }
            });
        }

        // Lấy tất cả khuyến mãi
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.KhuyenMais
                .Include(k => k.SanPhamKhuyenMais)
                .Select(k => new
                {
                    k.Id,
                    k.TenKhuyenMai,
                    k.MoTa,
                    k.PhanTramGiam,
                    k.NgayBatDau,
                    k.NgayKetThuc,
                    SoSanPham = k.SanPhamKhuyenMais.Count,
                    TrangThai = k.NgayKetThuc < DateOnly.FromDateTime(DateTime.Now) ? "Hết hạn" :
                               k.NgayBatDau > DateOnly.FromDateTime(DateTime.Now) ? "Sắp diễn ra" : "Đang diễn ra"
                })
                .ToListAsync();

            return Ok(new ApiRespone
            {
                Success = true,
                Message = "Lấy danh sách khuyến mãi thành công",
                Data = data
            });
        }

        // Lấy khuyến mãi đang active (đang diễn ra)
        [HttpGet("active")]
        public async Task<IActionResult> GetActive()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            var data = await _context.KhuyenMais
                .Where(k => k.NgayBatDau <= today && k.NgayKetThuc >= today)
                .Select(k => new
                {
                    k.Id,
                    k.TenKhuyenMai,
                    k.MoTa,
                    k.PhanTramGiam,
                    k.NgayBatDau,
                    k.NgayKetThuc
                })
                .ToListAsync();

            return Ok(new ApiRespone
            {
                Success = true,
                Message = "Lấy danh sách khuyến mãi đang active thành công",
                Data = data
            });
        }

        // Lấy khuyến mãi theo ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var khuyenMai = await _context.KhuyenMais
                .Include(k => k.SanPhamKhuyenMais)
                    .ThenInclude(spkm => spkm.SanPham)
                .Where(k => k.Id == id)
                .Select(k => new
                {
                    k.Id,
                    k.TenKhuyenMai,
                    k.MoTa,
                    k.PhanTramGiam,
                    k.NgayBatDau,
                    k.NgayKetThuc,
                    SanPhams = k.SanPhamKhuyenMais.Select(spkm => new
                    {
                        Id = spkm.SanPham.Id,
                        TenSanPham = spkm.SanPham.TenSanPham,
                        Gia = spkm.SanPham.Gia
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (khuyenMai == null)
            {
                return NotFound(new ApiRespone
                {
                    Success = false,
                    Message = "Không tìm thấy khuyến mãi"
                });
            }

            return Ok(new ApiRespone
            {
                Success = true,
                Message = "Lấy thông tin khuyến mãi thành công",
                Data = khuyenMai
            });
        }

        // Tạo khuyến mãi mới
        [HttpPost]
        public async Task<IActionResult> Create(KhuyenMaiRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiRespone
                {
                    Success = false,
                    Message = "Dữ liệu không hợp lệ: " + string.Join("; ", errors),
                    Data = ModelState
                });
            }

            if (request.NgayKetThuc < request.NgayBatDau)
            {
                return BadRequest(new ApiRespone
                {
                    Success = false,
                    Message = "Ngày kết thúc phải sau ngày bắt đầu"
                });
            }

            var khuyenMai = new KhuyenMai
            {
                TenKhuyenMai = request.TenKhuyenMai,
                MoTa = request.MoTa,
                PhanTramGiam = request.PhanTramGiam,
                NgayBatDau = request.NgayBatDau,
                NgayKetThuc = request.NgayKetThuc
            };

            _context.KhuyenMais.Add(khuyenMai);
            await _context.SaveChangesAsync();

            return Ok(new ApiRespone
            {
                Success = true,
                Message = "Tạo khuyến mãi thành công",
                Data = new
                {
                    khuyenMai.Id,
                    khuyenMai.TenKhuyenMai,
                    khuyenMai.MoTa,
                    khuyenMai.PhanTramGiam,
                    khuyenMai.NgayBatDau,
                    khuyenMai.NgayKetThuc
                }
            });
        }

        // Cập nhật khuyến mãi
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, KhuyenMaiRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiRespone
                {
                    Success = false,
                    Message = "Dữ liệu không hợp lệ: " + string.Join("; ", errors),
                    Data = ModelState
                });
            }

            var khuyenMai = await _context.KhuyenMais.FindAsync(id);
            if (khuyenMai == null)
            {
                return NotFound(new ApiRespone
                {
                    Success = false,
                    Message = "Không tìm thấy khuyến mãi"
                });
            }

            if (request.NgayKetThuc < request.NgayBatDau)
            {
                return BadRequest(new ApiRespone
                {
                    Success = false,
                    Message = "Ngày kết thúc phải sau ngày bắt đầu"
                });
            }

            khuyenMai.TenKhuyenMai = request.TenKhuyenMai;
            khuyenMai.MoTa = request.MoTa;
            khuyenMai.PhanTramGiam = request.PhanTramGiam;
            khuyenMai.NgayBatDau = request.NgayBatDau;
            khuyenMai.NgayKetThuc = request.NgayKetThuc;

            await _context.SaveChangesAsync();

            return Ok(new ApiRespone
            {
                Success = true,
                Message = "Cập nhật khuyến mãi thành công",
                Data = new
                {
                    khuyenMai.Id,
                    khuyenMai.TenKhuyenMai,
                    khuyenMai.MoTa,
                    khuyenMai.PhanTramGiam,
                    khuyenMai.NgayBatDau,
                    khuyenMai.NgayKetThuc
                }
            });
        }

        // Xóa khuyến mãi
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var khuyenMai = await _context.KhuyenMais
                .Include(k => k.SanPhamKhuyenMais)
                .FirstOrDefaultAsync(k => k.Id == id);

            if (khuyenMai == null)
            {
                return NotFound(new ApiRespone
                {
                    Success = false,
                    Message = "Không tìm thấy khuyến mãi"
                });
            }

            // Xóa tất cả sản phẩm liên kết
            _context.SanPhamKhuyenMais.RemoveRange(khuyenMai.SanPhamKhuyenMais);
            _context.KhuyenMais.Remove(khuyenMai);
            await _context.SaveChangesAsync();

            return Ok(new ApiRespone
            {
                Success = true,
                Message = "Xóa khuyến mãi thành công"
            });
        }

        // Gán sản phẩm vào khuyến mãi
        [HttpPost("gan-san-pham")]
        public async Task<IActionResult> GanSanPham(GanSanPhamKhuyenMaiRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiRespone
                {
                    Success = false,
                    Message = "Dữ liệu không hợp lệ: " + string.Join("; ", errors)
                });
            }

            // Kiểm tra sản phẩm và khuyến mãi có tồn tại không
            var sanPham = await _context.SanPhams.FindAsync(request.SanPhamId);
            if (sanPham == null)
            {
                return NotFound(new ApiRespone
                {
                    Success = false,
                    Message = "Không tìm thấy sản phẩm"
                });
            }

            var khuyenMai = await _context.KhuyenMais.FindAsync(request.KhuyenMaiId);
            if (khuyenMai == null)
            {
                return NotFound(new ApiRespone
                {
                    Success = false,
                    Message = "Không tìm thấy khuyến mãi"
                });
            }

            // Kiểm tra đã tồn tại chưa
            var existing = await _context.SanPhamKhuyenMais
                .FirstOrDefaultAsync(spkm => spkm.SanPhamId == request.SanPhamId && spkm.KhuyenMaiId == request.KhuyenMaiId);

            if (existing != null)
            {
                return BadRequest(new ApiRespone
                {
                    Success = false,
                    Message = "Sản phẩm đã được gán vào khuyến mãi này rồi"
                });
            }

            var sanPhamKhuyenMai = new SanPhamKhuyenMai
            {
                SanPhamId = request.SanPhamId,
                KhuyenMaiId = request.KhuyenMaiId
            };

            _context.SanPhamKhuyenMais.Add(sanPhamKhuyenMai);
            await _context.SaveChangesAsync();

            return Ok(new ApiRespone
            {
                Success = true,
                Message = "Gán sản phẩm vào khuyến mãi thành công",
                Data = new
                {
                    Id = sanPhamKhuyenMai.Id,
                    SanPhamId = sanPhamKhuyenMai.SanPhamId,
                    KhuyenMaiId = sanPhamKhuyenMai.KhuyenMaiId
                }
            });
        }

        // Xóa sản phẩm khỏi khuyến mãi
        [HttpDelete("xoa-san-pham/{sanPhamId}/{khuyenMaiId}")]
        public async Task<IActionResult> XoaSanPham(int sanPhamId, int khuyenMaiId)
        {
            var sanPhamKhuyenMai = await _context.SanPhamKhuyenMais
                .FirstOrDefaultAsync(spkm => spkm.SanPhamId == sanPhamId && spkm.KhuyenMaiId == khuyenMaiId);

            if (sanPhamKhuyenMai == null)
            {
                return NotFound(new ApiRespone
                {
                    Success = false,
                    Message = "Không tìm thấy liên kết sản phẩm - khuyến mãi"
                });
            }

            _context.SanPhamKhuyenMais.Remove(sanPhamKhuyenMai);
            await _context.SaveChangesAsync();

            return Ok(new ApiRespone
            {
                Success = true,
                Message = "Xóa sản phẩm khỏi khuyến mãi thành công"
            });
        }

        // Lấy danh sách sản phẩm trong khuyến mãi
        [HttpGet("{id}/san-pham")]
        public async Task<IActionResult> GetSanPhamByKhuyenMai(int id)
        {
            var khuyenMai = await _context.KhuyenMais.FindAsync(id);
            if (khuyenMai == null)
            {
                return NotFound(new ApiRespone
                {
                    Success = false,
                    Message = "Không tìm thấy khuyến mãi"
                });
            }

            var sanPhams = await _context.SanPhamKhuyenMais
                .Include(spkm => spkm.SanPham)
                    .ThenInclude(sp => sp.HinhAnhSanPhams)
                .Where(spkm => spkm.KhuyenMaiId == id)
                .Select(spkm => new
                {
                    Id = spkm.SanPham.Id,
                    TenSanPham = spkm.SanPham.TenSanPham,
                    Gia = spkm.SanPham.Gia,
                    GiaGiam = spkm.SanPham.GiaGiam,
                    HinhAnhDaiDien = spkm.SanPham.HinhAnhSanPhams.Select(h => h.HinhAnh).FirstOrDefault()
                })
                .ToListAsync();

            return Ok(new ApiRespone
            {
                Success = true,
                Message = "Lấy danh sách sản phẩm thành công",
                Data = sanPhams
            });
        }
    }
}

