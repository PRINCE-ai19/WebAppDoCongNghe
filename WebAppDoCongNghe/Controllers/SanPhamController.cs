using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppDoCongNghe.Models.ApiRespone;
using WebAppDoCongNghe.Models.Entity;
using WebAppDoCongNghe.Models.model;
using WebAppDoCongNghe.Service;

namespace WebAppDoCongNghe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SanPhamController : ControllerBase
    {

        private readonly WebAppDoCongNgheContext _context;

        private readonly ICloudinaryService _cloudinaryService;
        public SanPhamController(WebAppDoCongNgheContext context, ICloudinaryService cloudinaryService)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
        }

        // Helper method để tính giá giảm từ khuyến mãi đang active
        private decimal? TinhGiaGiamTuKhuyenMai(decimal giaGoc, int sanPhamId)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            
            // Lấy khuyến mãi đang active cho sản phẩm này
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
                // Tính giá giảm: giá gốc * (1 - phần trăm giảm / 100)
                return giaGoc * (1 - khuyenMaiActive / 100);
            }

            return null;
        }


        [HttpGet("paging")]
        public IActionResult GetPaging(int page , int pageSize )
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            
            var query = _context.SanPhams
                .Include(p => p.DanhMuc)
                .Include(p => p.HinhAnhSanPhams)
                .Include(p => p.SanPhamKhuyenMais)
                    .ThenInclude(spkm => spkm.KhuyenMai)
                .ToList()
                .Select(p => {
                    // Tính giá giảm từ khuyến mãi đang active
                    var khuyenMaiActive = p.SanPhamKhuyenMais
                        .Where(spkm => spkm.KhuyenMai != null
                            && spkm.KhuyenMai.NgayBatDau <= today 
                            && spkm.KhuyenMai.NgayKetThuc >= today
                            && spkm.KhuyenMai.PhanTramGiam.HasValue)
                        .Select(spkm => spkm.KhuyenMai.PhanTramGiam.Value)
                        .OrderByDescending(pt => pt)
                        .FirstOrDefault();

                    var giaGiamTuKhuyenMai = khuyenMaiActive > 0 
                        ? p.Gia * (1 - khuyenMaiActive / 100) 
                        : (decimal?)null;

                    // Ưu tiên giá giảm từ khuyến mãi, nếu không có thì dùng giá giảm cũ
                    var giaGiamCuoiCung = giaGiamTuKhuyenMai ?? p.GiaGiam;

                    return new
                    {
                        p.Id,
                        p.TenSanPham,
                        p.ThuongHieu,
                        p.Gia,
                        GiaGiam = giaGiamCuoiCung,
                        p.SoLuongTon,
                        p.MoTa,
                        p.HienThi,
                        HinhAnhDaiDien = p.HinhAnhSanPhams.Select(r => r.HinhAnh).FirstOrDefault(),
                        HinhAnh = p.HinhAnhSanPhams.Select(r => new
                        {
                            r.Id,
                            r.SanPhamId,
                            r.HinhAnh,
                        }).ToList(),
                        p.NgayThem,
                        DanhMuc = p.DanhMuc != null ? p.DanhMuc.TenDanhMuc : null
                    };
                })
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
                message = "Lấy danh sách sản phẩm thành công",
                data = new
                {
                    items = items,
                    total = total,
                    page = page,
                    pageSize = pageSize
                }
            });
        }
       [HttpGet]
        public IActionResult GetAll()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            
            var products = _context.SanPhams
                .Include(p => p.DanhMuc)
                .Include(p => p.HinhAnhSanPhams)
                .Include(p => p.SanPhamKhuyenMais)
                    .ThenInclude(spkm => spkm.KhuyenMai)
                .ToList()
                .Select(p => {
                    // Tính giá giảm từ khuyến mãi đang active
                    var khuyenMaiActive = p.SanPhamKhuyenMais
                        .Where(spkm => spkm.KhuyenMai != null
                            && spkm.KhuyenMai.NgayBatDau <= today 
                            && spkm.KhuyenMai.NgayKetThuc >= today
                            && spkm.KhuyenMai.PhanTramGiam.HasValue)
                        .Select(spkm => spkm.KhuyenMai.PhanTramGiam.Value)
                        .OrderByDescending(pt => pt)
                        .FirstOrDefault();

                    var giaGiamTuKhuyenMai = khuyenMaiActive > 0 
                        ? p.Gia * (1 - khuyenMaiActive / 100) 
                        : (decimal?)null;

                    // Ưu tiên giá giảm từ khuyến mãi, nếu không có thì dùng giá giảm cũ
                    var giaGiamCuoiCung = giaGiamTuKhuyenMai ?? p.GiaGiam;

                    return new
                    {
                        p.Id,
                        p.TenSanPham,
                        p.ThuongHieu,
                        p.Gia,
                        GiaGiam = giaGiamCuoiCung,
                        p.SoLuongTon,
                        p.MoTa,
                        p.HienThi,
                        HinhAnhDaiDien = p.HinhAnhSanPhams.Select(r => r.HinhAnh).FirstOrDefault(),
                        p.NgayThem,
                        DanhMuc = p.DanhMuc != null ? p.DanhMuc.TenDanhMuc : null
                    };
                })
                .ToList();

            return Ok(new ApiRespone
            {
                Success = true,
                Message = "Danh sách sản phẩm",
                Data = products
            });
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            
            var product = _context.SanPhams
                .Include(p => p.DanhMuc)
                .Include(p => p.HinhAnhSanPhams)
                .Include(p => p.CauHinhSanPhams)
                .Include(p => p.SanPhamKhuyenMais)
                    .ThenInclude(spkm => spkm.KhuyenMai)
                .FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound(new ApiRespone
                {
                    Success = false,
                    Message = "Không tìm thấy sản phẩm"
                });
            }

            // Tính giá giảm từ khuyến mãi đang active
            var khuyenMaiActive = product.SanPhamKhuyenMais
                .Where(spkm => spkm.KhuyenMai != null
                    && spkm.KhuyenMai.NgayBatDau <= today 
                    && spkm.KhuyenMai.NgayKetThuc >= today
                    && spkm.KhuyenMai.PhanTramGiam.HasValue)
                .Select(spkm => spkm.KhuyenMai.PhanTramGiam.Value)
                .OrderByDescending(pt => pt)
                .FirstOrDefault();

            var giaGiamTuKhuyenMai = khuyenMaiActive > 0 
                ? product.Gia * (1 - khuyenMaiActive / 100) 
                : (decimal?)null;

            // Ưu tiên giá giảm từ khuyến mãi, nếu không có thì dùng giá giảm cũ
            var giaGiamCuoiCung = giaGiamTuKhuyenMai ?? product.GiaGiam;

            var imageList = product.HinhAnhSanPhams?
                                   .Select(img => img.HinhAnh) // lấy URL ảnh Cloudinary
                                  .ToList();

            var cauHinhList = product.CauHinhSanPhams?
                .Select(ch => new
                {
                    Id = ch.Id,
                    TenThongSo = ch.TenThongSo,
                    GiaTri = ch.GiaTri
                })
                .ToList();

            var result = new
            {
                Id = product.Id,
                TenSanPham = product.TenSanPham,
                Gia = product.Gia,
                GiaGiam = giaGiamCuoiCung,
                MoTa = product.MoTa,
                SoLuongTon = product.SoLuongTon,
                NgayThem = product.NgayThem,
                ThuongHieu = product.ThuongHieu,
                DanhMuc = product.DanhMuc?.TenDanhMuc,
                Hienthi = product.HienThi,
                HinhAnhList = imageList,
                CauHinhSanPhams = cauHinhList
            };


            return Ok(new ApiRespone
            {
                Success = true,
                Message = "Lấy thông tin sản phẩm thành công",
                Data = result
            });
        }


        [HttpGet("category/{categoryId}")]
        public IActionResult GetByCategory(int categoryId)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            
            var products = _context.SanPhams
               .Where(p => p.DanhMucId == categoryId)
               .Include(p => p.DanhMuc)
               .Include(p => p.HinhAnhSanPhams)
               .Include(p => p.SanPhamKhuyenMais)
                   .ThenInclude(spkm => spkm.KhuyenMai)
               .ToList();


            if (products == null || products.Count == 0)
            {
                return NotFound(new ApiRespone
                {
                    Success = false,
                    Message = "Không có sản phẩm nào thuộc danh mục này"
                });
            }


            var result = products.Select(p => {
                // Tính giá giảm từ khuyến mãi đang active
                var khuyenMaiActive = p.SanPhamKhuyenMais
                    .Where(spkm => spkm.KhuyenMai != null
                        && spkm.KhuyenMai.NgayBatDau <= today 
                        && spkm.KhuyenMai.NgayKetThuc >= today
                        && spkm.KhuyenMai.PhanTramGiam.HasValue)
                    .Select(spkm => spkm.KhuyenMai.PhanTramGiam.Value)
                    .OrderByDescending(pt => pt)
                    .FirstOrDefault();

                var giaGiamTuKhuyenMai = khuyenMaiActive > 0 
                    ? p.Gia * (1 - khuyenMaiActive / 100) 
                    : (decimal?)null;

                // Ưu tiên giá giảm từ khuyến mãi, nếu không có thì dùng giá giảm cũ
                var giaGiamCuoiCung = giaGiamTuKhuyenMai ?? p.GiaGiam;

                return new
                {
                    Id = p.Id,
                    TenSanPham = p.TenSanPham,
                    Gia = p.Gia,
                    GiaGiam = giaGiamCuoiCung,
                    MoTa = p.MoTa,
                    SoLuongTon = p.SoLuongTon,
                    NgayThem = p.NgayThem,
                    ThuongHieu = p.ThuongHieu,
                    DanhMuc = p.DanhMuc?.TenDanhMuc,

                    // Ảnh đại diện: lấy URL đầu tiên trong danh sách hình của sản phẩm
                    AnhDaiDien = p.HinhAnhSanPhams != null && p.HinhAnhSanPhams.Any()
                   ? p.HinhAnhSanPhams.First().HinhAnh
                   : null
                };
            }).ToList();


            return Ok(new ApiRespone
            {
                Success = true,
                Message = "Lấy danh sách sản phẩm theo danh mục thành công",
                Data = result
            });
        }


        [HttpPost("ThemSp")]
        public async Task<IActionResult> Create([FromForm] SannPhamBind model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiRespone
                {
                    Success = false,
                    Message = "Dữ liệu không hợp lệ"
                });
            }

            using var trans = await _context.Database.BeginTransactionAsync();
            try
            {
                var sp = new SanPham
                {
                    TenSanPham = model.TenSanPham,
                    DanhMucId = model.DanhMucId,
                    ThuongHieu = model.ThuongHieu,
                    Gia = model.Gia,
                    SoLuongTon = model.SoLuongTon,
                    MoTa = model.MoTa,
                    NgayThem = DateTime.Now
                };

                _context.SanPhams.Add(sp);
                await _context.SaveChangesAsync();

                // Upload ảnh nếu có
                if (model.Hinhanh != null && model.Hinhanh.Any())
                {
                    foreach (var file in model.Hinhanh)
                    {
                        var url = await _cloudinaryService.UploadImageAsync(file, "SanPham");
                        if (url != null)
                        {
                            _context.HinhAnhSanPhams.Add(new HinhAnhSanPham
                            {
                                SanPhamId = sp.Id,
                                HinhAnh = url
                            });
                        }
                    }

                    await _context.SaveChangesAsync();
                }

                await trans.CommitAsync();

                return Ok(new
                {
                    Success = true,
                    Message = "Thêm sản phẩm thành công"
                });
            }
            catch (Exception ex)
            {
                await trans.RollbackAsync();
                return BadRequest(ex.Message);
            }
        }


        [HttpPut("SuaSp/{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] SannPhamBind model)
        {
            var product = await _context.SanPhams
                .Include(x => x.HinhAnhSanPhams)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (product == null)
                return NotFound(new ApiRespone { Success = false, Message = "Không tìm thấy sản phẩm" });

            using var trans = await _context.Database.BeginTransactionAsync();
            try
            {
                product.TenSanPham = model.TenSanPham;
                product.DanhMucId = model.DanhMucId;
                product.ThuongHieu = model.ThuongHieu;
                product.Gia = model.Gia;
                product.SoLuongTon = model.SoLuongTon;
                product.MoTa = model.MoTa;
              

                _context.SanPhams.Update(product);
                await _context.SaveChangesAsync();

                // THÊM ẢNH MỚI
                if (model.Hinhanh != null && model.Hinhanh.Any())
                {
                    foreach (var file in model.Hinhanh)
                    {
                        var url = await _cloudinaryService.UploadImageAsync(file, "SanPham");
                        if (url != null)
                        {
                            _context.HinhAnhSanPhams.Add(new HinhAnhSanPham
                            {
                                SanPhamId = product.Id,
                                HinhAnh = url
                            });
                        }
                    }

                    await _context.SaveChangesAsync();
                }

                await trans.CommitAsync();

                return Ok(new ApiRespone
                {
                    Success = true,
                    Message = "Cập nhật sản phẩm thành công"
                });
            }
            catch (Exception ex)
            {
                await trans.RollbackAsync();
                return BadRequest(new { error = ex.Message });
            }
        }



        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var product = _context.SanPhams.Find(id);
            if (product == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Không tìm thấy sản phẩm"
                });
            }

            _context.SanPhams.Remove(product);
            _context.SaveChanges();

            return Ok(new
            {
                Success = true,
                Message = "Xóa sản phẩm thành công"
            });
        }


        [HttpDelete("DeleteImage/{imageId}")]
        public async Task<IActionResult> DeleteImage(int imageId)
        {
            var img = await _context.HinhAnhSanPhams.FindAsync(imageId);
            if (img == null)
            {
                return NotFound(new ApiRespone
                {
                    Success = false,
                    Message = "Không tìm thấy ảnh"
                });
            }

            _context.HinhAnhSanPhams.Remove(img);
            await _context.SaveChangesAsync();

            return Ok(new ApiRespone
            {
                Success = true,
                Message = "Xoá ảnh thành công"
            });
        }


        [HttpGet("search")]
        public IActionResult Search(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return BadRequest(new ApiRespone
                {
                    Success = false,
                    Message = "Vui lòng nhập từ khóa tìm kiếm."
                });
            }

            var today = DateOnly.FromDateTime(DateTime.Now);

            var products = _context.SanPhams
                .Include(p => p.DanhMuc)
                .Include(p => p.HinhAnhSanPhams)
                .Include(p => p.SanPhamKhuyenMais)
                    .ThenInclude(spkm => spkm.KhuyenMai)
                .Where(p =>
                    EF.Functions.Like(p.TenSanPham, $"%{keyword}%") ||
                    EF.Functions.Like(p.ThuongHieu, $"%{keyword}%") ||
                    EF.Functions.Like(p.MoTa, $"%{keyword}%"))
                .ToList()
                .Select(p => {
                    // Tính giá giảm từ khuyến mãi đang active
                    var khuyenMaiActive = p.SanPhamKhuyenMais
                        .Where(spkm => spkm.KhuyenMai != null
                            && spkm.KhuyenMai.NgayBatDau <= today 
                            && spkm.KhuyenMai.NgayKetThuc >= today
                            && spkm.KhuyenMai.PhanTramGiam.HasValue)
                        .Select(spkm => spkm.KhuyenMai.PhanTramGiam.Value)
                        .OrderByDescending(pt => pt)
                        .FirstOrDefault();

                    var giaGiamTuKhuyenMai = khuyenMaiActive > 0 
                        ? p.Gia * (1 - khuyenMaiActive / 100) 
                        : (decimal?)null;

                    // Ưu tiên giá giảm từ khuyến mãi, nếu không có thì dùng giá giảm cũ
                    var giaGiamCuoiCung = giaGiamTuKhuyenMai ?? p.GiaGiam;

                    return new
                    {
                        p.Id,
                        p.TenSanPham,
                        p.ThuongHieu,
                        p.Gia,
                        GiaGiam = giaGiamCuoiCung,
                        p.SoLuongTon,
                        p.MoTa,
                        HinhAnhDaiDien = p.HinhAnhSanPhams.Select(r => r.HinhAnh).FirstOrDefault(),
                        p.NgayThem,
                        DanhMuc = p.DanhMuc != null ? p.DanhMuc.TenDanhMuc : null
                    };
                })
                .ToList();

            if (!products.Any())
            {
                return NotFound(new ApiRespone
                {
                    Success = false,
                    Message = "Không tìm thấy sản phẩm nào phù hợp với từ khóa tìm kiếm."
                });
            }

            return Ok(new ApiRespone
            {
                Success = true,
                Message = $"Kết quả tìm kiếm cho '{keyword}'",
                Data = products
            });
        }


        [HttpGet("GetByTaiKhoan/{taiKhoanId}")]
        public async Task<IActionResult> GetByTaiKhoan(int taiKhoanId)
        {
            var yeuThichList = await _context.SanPhamYeuThiches
                .Where(x => x.TaiKhoanId == taiKhoanId)
                .Select(x => x.SanPhamId)
                .ToListAsync();

            return Ok(yeuThichList);
        }

      
        [HttpPost("toggle")]
        public async Task<IActionResult> ToggleYeuThich(int taiKhoanId, int sanPhamId)
        {
            var existing = await _context.SanPhamYeuThiches
                .FirstOrDefaultAsync(x => x.TaiKhoanId == taiKhoanId && x.SanPhamId == sanPhamId);

            if (existing == null)
            {
                var newFav = new SanPhamYeuThich
                {
                    TaiKhoanId = taiKhoanId,
                    SanPhamId = sanPhamId,
                };
                _context.SanPhamYeuThiches.Add(newFav);
                await _context.SaveChangesAsync();
                return Ok(new { isFavorite = true });
            }
            else
            {
                _context.SanPhamYeuThiches.Remove(existing);
                await _context.SaveChangesAsync();
                return Ok(new { isFavorite = false });
            }
        }
    }
}
