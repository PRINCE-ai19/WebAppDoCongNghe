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
    public class LienHeController : ControllerBase
    {
        private readonly WebAppDoCongNgheContext _context;
        private readonly PublicService _publicService;
        private readonly IConfiguration _configuration;

        public LienHeController(WebAppDoCongNgheContext context, PublicService publicService, IConfiguration configuration)
        {
            _context = context;
            _publicService = publicService;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> GuiLienHe([FromBody] LienHeRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiRespone
                    {
                        Success = false,
                        Message = "Dữ liệu không hợp lệ",
                        Data = ModelState
                    });
                }

                // Lưu liên hệ vào database
                var lienHe = new LienHe
                {
                    HoTen = request.HoTen,
                    Email = request.Email,
                    SoDienThoai = request.SoDienThoai,
                    NoiDung = request.NoiDung,
                    NgayGui = DateTime.Now
                };

                _context.LienHes.Add(lienHe);
                await _context.SaveChangesAsync();

                // Gửi email thông báo đến admin
                var adminEmail = _configuration["EmailSettings:From"] ?? "luanpham45794@gmail.com";
                var subject = $"Liên hệ mới từ {request.HoTen}";
                var body = $@"
Bạn có một liên hệ mới từ website:

Họ tên: {request.HoTen}
Email: {request.Email}
Số điện thoại: {request.SoDienThoai ?? "Không có"}
Ngày gửi: {DateTime.Now:dd/MM/yyyy HH:mm:ss}

Nội dung:
{request.NoiDung}

---
Email này được gửi tự động từ hệ thống TechStore.
";

                try
                {
                    await _publicService.SendEmailAsync(adminEmail, subject, body);
                }
                catch (Exception emailEx)
                {
                    // Log lỗi gửi email nhưng vẫn trả về thành công vì đã lưu vào database
                    Console.WriteLine($"Lỗi khi gửi email: {emailEx.Message}");
                }

                return Ok(new ApiRespone
                {
                    Success = true,
                    Message = "Gửi liên hệ thành công! Chúng tôi sẽ phản hồi sớm nhất có thể.",
                    Data = new
                    {
                        lienHe.Id,
                        lienHe.HoTen,
                        lienHe.Email,
                        lienHe.SoDienThoai,
                        lienHe.NgayGui
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiRespone
                {
                    Success = false,
                    Message = "Lỗi khi xử lý liên hệ: " + ex.Message
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var list = await _context.LienHes
                    .OrderByDescending(lh => lh.NgayGui)
                    .Select(lh => new
                    {
                        lh.Id,
                        lh.HoTen,
                        lh.Email,
                        lh.SoDienThoai,
                        lh.NoiDung,
                        lh.NgayGui
                    })
                    .ToListAsync();

                return Ok(new ApiRespone
                {
                    Success = true,
                    Message = "Lấy danh sách liên hệ thành công",
                    Data = list
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiRespone
                {
                    Success = false,
                    Message = "Lỗi khi lấy danh sách liên hệ: " + ex.Message
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var lienHe = await _context.LienHes.FindAsync(id);

                if (lienHe == null)
                {
                    return NotFound(new ApiRespone
                    {
                        Success = false,
                        Message = "Không tìm thấy liên hệ"
                    });
                }

                return Ok(new ApiRespone
                {
                    Success = true,
                    Message = "Lấy thông tin liên hệ thành công",
                    Data = new
                    {
                        lienHe.Id,
                        lienHe.HoTen,
                        lienHe.Email,
                        lienHe.SoDienThoai,
                        lienHe.NoiDung,
                        lienHe.NgayGui
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiRespone
                {
                    Success = false,
                    Message = "Lỗi khi lấy thông tin liên hệ: " + ex.Message
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> XoaLienHe(int id)
        {
            try
            {
                var lienHe = await _context.LienHes.FindAsync(id);

                if (lienHe == null)
                {
                    return NotFound(new ApiRespone
                    {
                        Success = false,
                        Message = "Không tìm thấy liên hệ cần xóa"
                    });
                }

                _context.LienHes.Remove(lienHe);
                await _context.SaveChangesAsync();

                return Ok(new ApiRespone
                {
                    Success = true,
                    Message = "Xóa liên hệ thành công"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiRespone
                {
                    Success = false,
                    Message = "Lỗi khi xóa liên hệ: " + ex.Message
                });
            }
        }
    }
}

