using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAppDoCongNghe.Models.ApiRespone;
using WebAppDoCongNghe.Models.Entity;
using WebAppDoCongNghe.Models.model;
using WebAppDoCongNghe.Service;

namespace WebAppDoCongNghe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TinTucController : ControllerBase
    {
        private readonly WebAppDoCongNgheContext _context;

        private readonly ICloudinaryService _cloudinaryService;
        public TinTucController(WebAppDoCongNgheContext context, ICloudinaryService cloudinaryService)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
        }

        [HttpGet("paging")]
        public IActionResult AdminGetAll(int page, int pageSize)
        {
            var item = _context.TinTucs.ToList().AsQueryable();

            int total = item.Count();

            var items = item
                      .OrderByDescending(x => x.Id)
                      .Skip((page - 1) * pageSize)
                      .Take(pageSize)
                      .ToList();
            return Ok(new
            {
                success = true,
                message = "Lấy danh sách tin tức thành công",
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
            var items = _context.TinTucs.ToList();
            return Ok(new ApiRespone
            {
                Success = true,
                Message = "Các tin tức sản phẩm",
                Data = items
            });
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var tin = _context.TinTucs.Find(id);
            if (tin == null)
                return NotFound("Không tìm thấy tin tức");

            return Ok(new ApiRespone
            {
                Success = true,
                Message = "Các tin tức sản phẩm",
                Data = tin
            }
                );
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromForm] TinTucRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiRespone
                {
                    Success = false,
                    Message = "Dữ liệu không hợp lệ"
                });
            }

            string? imageUrl = null;

            if (request.Image != null)
            {
                imageUrl = await _cloudinaryService.UploadImageAsync(request.Image, "TinTuc");
            }

            var tin = new TinTuc
            {
                TieuDe = request.TieuDe,
                MoTa = request.MoTa,
                NoiDung = request.NoiDung,
                Image = imageUrl,
                NgayTao = DateTime.Now,
                HienThi = request.HienThi ?? true
            };

            _context.TinTucs.Add(tin);
            await _context.SaveChangesAsync();

            return Ok(new ApiRespone
            {
                Success = true,
                Message = "Thêm tin tức thành công",
                Data = tin
            });
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromForm] TinTucRequest request)
        {
            var tin = await _context.TinTucs.FindAsync(id);

            if (tin == null)
            {
                return NotFound(new ApiRespone
                {
                    Success = false,
                    Message = "Tin tức không tồn tại"
                });
            }

            string? imageUrl = tin.Image;

            if (request.Image != null)
            {
                imageUrl = await _cloudinaryService.UploadImageAsync(request.Image, "TinTuc");
            }

            tin.TieuDe = request.TieuDe;
            tin.MoTa = request.MoTa;
            tin.NoiDung = request.NoiDung;
            tin.Image = imageUrl;
            tin.HienThi = request.HienThi ?? tin.HienThi;
            tin.NgaySua = DateTime.Now;

            _context.TinTucs.Update(tin);
            await _context.SaveChangesAsync();

            return Ok(new ApiRespone
            {
                Success = true,
                Message = "Cập nhật tin tức thành công",
                Data = tin
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var tin = await _context.TinTucs.FindAsync(id);

            if (tin == null)
            {
                return NotFound(new ApiRespone
                {
                    Success = false,
                    Message = "Tin tức không tồn tại"
                });
            }

            _context.TinTucs.Remove(tin);
            await _context.SaveChangesAsync();

            return Ok(new ApiRespone
            {
                Success = true,
                Message = "Xoá tin tức thành công"
            });
        }


    }
}
