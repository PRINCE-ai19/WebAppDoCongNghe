
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAppDoCongNghe.Models.ApiRespone;
using WebAppDoCongNghe.Models.Entities;
using WebAppDoCongNghe.Models.model;
using WebAppDoCongNghe.Service;

namespace WebAppDoCongNghe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DangKyController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly PublicService _publicService;

        public DangKyController(AppDbContext context, PublicService publicService)
        {
            _context = context;
            _publicService = publicService;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] DangKy request)
        {
            //  Ki?m tra d? li?u d?u vào
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiRespone
                {
                    Success = false,
                    Message = "D? li?u nh?p vào không h?p l?, vui lòng ki?m tra l?i."
                });
            }

            //  Ki?m tra email dã t?n t?i 
            var existingEmail = _context.TaiKhoans.FirstOrDefault(u => u.Email == request.Email);
            if (existingEmail != null)
            {
                return BadRequest(new ApiRespone
                {
                    Success = false,
                    Message = "Email dã du?c s? d?ng."
                });
            }

            //  Ki?m tra  s? di?n thoai dã t?n t?i
            var existingPhone = _context.TaiKhoans.FirstOrDefault(u => u.SoDienThoai == request.SoDienThoai);
            if (existingPhone != null)
            {
                return BadRequest(new ApiRespone
                {
                    Success = false,
                    Message = "S? di?n tho?i dã du?c s? d?ng."
                });
            }
            // T?o m?i user 
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
                Message = "Ðang ký thành công , Gi? hàng c?a b?n dã du?c t?o! Vui lòng dang nh?p d? ti?p t?c.",
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

