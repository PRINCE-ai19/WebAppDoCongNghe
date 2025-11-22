using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppDoCongNghe.Models.ApiRespone;
using WebAppDoCongNghe.Models.Entity;
using WebAppDoCongNghe.Models.model;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace WebAppDoCongNghe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DanhMucController : ControllerBase
    {
        private readonly WebAppDoCongNgheContext _context;
        public DanhMucController( WebAppDoCongNgheContext context)
        {
            _context = context;
        }

        [HttpGet("paging")]
        public IActionResult GetPagingDM(int page, int pageSize)
        {
            var item = _context.DanhMucs.ToList().AsQueryable();

            int total = item.Count();

            var items = item
                      .OrderByDescending(x => x.Id)
                      .Skip((page - 1) * pageSize)
                      .Take(pageSize)
                      .ToList();
            return Ok(new
            {
                success = true,
                message = "Lấy danh sách danh mục thành công",
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
            var item = _context.DanhMucs.ToList();
            return Ok(new ApiRespone
            {
                Success = true,
                Message = "Các danh mục sản phẩm",
                Data = item
            });

        }

        // lấy theo id
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var danhMuc = _context.DanhMucs.Find(id);
            if (danhMuc == null)
            {
                return NotFound(new ApiRespone
                {
                    Success = false,
                    Message = "Không tìm thấy danh mục sản phẩm"
                });
            }
            return Ok(new ApiRespone
            {
                Success = true,
                Message = "lấy danh mục các sản phẩm thành công",
                Data = danhMuc
            });
        }

        [HttpPost]
        public IActionResult Create([FromBody] DanhMucRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiRespone
                {
                    Success = false,
                    Message = "dữ liệu không hợp lệ"
                });

            var danhMuc = new DanhMuc
            {
                TenDanhMuc = model.TenDanhMuc,
                MoTa = model.MoTa,
                ViTri = model.ViTri
            };

            _context.DanhMucs.Add(danhMuc);
            _context.SaveChanges();

            return Ok(new ApiRespone
            {
                Success = true,
                Message = "Thêm danh mục thành công",
                Data = model
            });
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] DanhMucRequest model)
        {
            var danhMuc = _context.DanhMucs.Find(id);
            if (danhMuc == null)
                return NotFound(new ApiRespone { Success = false, Message = "Không tìm thấy danh mục" });

            danhMuc.TenDanhMuc = model.TenDanhMuc;
            danhMuc.MoTa = model.MoTa;
            danhMuc.ViTri = model.ViTri;

            _context.SaveChanges();

            return Ok(new ApiRespone { Success = true, Message = "Cập nhật danh mục thành công", Data = danhMuc });
        }


        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var danhMuc = _context.DanhMucs.Find(id);
            if (danhMuc == null)
                return NotFound(new ApiRespone { Success = false, Message = "Không tìm thấy danh mục" });

            _context.DanhMucs.Remove(danhMuc);
            _context.SaveChanges();

            return Ok(new ApiRespone { Success = true, Message = "Xóa danh mục thành công" });
        }


        [HttpGet("{id}/products")]
        public IActionResult GetProductsByCategory(int id)
        {
            var danhMuc = _context.DanhMucs.Include(dm => dm.SanPhams).FirstOrDefault(dm => dm.Id == id);
            if (danhMuc == null)
            {
                return NotFound(new { Success = false, Message = "Không tìm thấy danh mục" });
            }

            return Ok(new
            {
                Success = true,
                Message = $"Danh sách sản phẩm thuộc danh mục {danhMuc.TenDanhMuc}",
                Data = danhMuc.SanPhams
            });
        }
    }
}
