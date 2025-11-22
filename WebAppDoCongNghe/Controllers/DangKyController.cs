
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
    public class DangKyController : ControllerBase
    {
        private readonly WebAppDoCongNgheContext _context;
        private readonly PublicService _publicService;

        public DangKyController(WebAppDoCongNgheContext context, PublicService publicService)
        {
            _context = context;
            _publicService = publicService;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] DangKy request)
        {
            //  Kiểm tra dữ liệu đầu vào
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiRespone
                {
                    Success = false,
                    Message = "Dữ liệu nhập vào không hợp lệ, vui lòng kiểm tra lại."
                });
            }

            //  Kiểm tra email đã tồn tại 
            var existingEmail = _context.TaiKhoans.FirstOrDefault(u => u.Email == request.Email);
            if (existingEmail != null)
            {
                return BadRequest(new ApiRespone
                {
                    Success = false,
                    Message = "Email đã được sử dụng."
                });
            }

            //  Kiểm tra  số điện thoai đã tồn tại
            var existingPhone = _context.TaiKhoans.FirstOrDefault(u => u.SoDienThoai == request.SoDienThoai);
            if (existingPhone != null)
            {
                return BadRequest(new ApiRespone
                {
                    Success = false,
                    Message = "Số điện thoại đã được sử dụng."
                });
            }
            // Tạo mới user 
            var newUser = new TaiKhoan
            {
                HoTen = request.HoTen,
                SoDienThoai = request.SoDienThoai,
                Email = request.Email,
                MatKhau = _publicService.HashPassword(request.MatKhau),
                NgayDangKy = DateTime.Now,
                IdLoaiTaiKhoan = 2,
            };
            _context.TaiKhoans.Add(newUser);
            _context.SaveChanges();

            var newCart = new GioHang
            {
                TaiKhoanId = newUser.Id,
                NgayTao = DateTime.Now
            };

            _context.GioHangs.Add(newCart);
            _context.SaveChanges();

            return Ok(new ApiRespone
            {
                Success = true,
                Message = "Đăng ký thành công , Giỏ hàng của bạn đã được tạo! Vui lòng đăng nhập để tiếp tục.",
                Data = new
                {
                    newUser.Id,
                    newUser.HoTen,
                    newUser.Email,
                    newUser.SoDienThoai,
                    GioHangId = newCart.Id
                }

            });
        }
    }
}

