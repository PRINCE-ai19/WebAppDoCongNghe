
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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
    public class DangNhapController : ControllerBase
    {
        private readonly WebAppDoCongNgheContext _context;

        private readonly PublicService _publicService;

        private readonly JwtService _jwtService;

        public DangNhapController(WebAppDoCongNgheContext context , PublicService publicService , JwtService jwtService)
        {
            _publicService = publicService;
            _context = context;
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] DangNhap request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiRespone
                {
                    Success = false,
                    Message = "dữ liệu nhập vào đã sai kiểm tra lại"
                });
            }

            //  Tìm tài khoản trong database
            var user = _context.TaiKhoans.FirstOrDefault(u => u.Email == request.TaiKhoan || u.SoDienThoai == request.TaiKhoan);
            if (user == null)
            {
                return NotFound(new ApiRespone
                {
                    Success = false,
                    Message = "Sai email ."
                });
            }

            // so sánh mã hóa mật khẩu 
            var result = _publicService.PasswordVerification(user.MatKhau, request.MatKhau);
            if (result == PasswordVerificationResult.Failed)
            {
                return NotFound(new ApiRespone
                {
                    Success = false,
                    Message = "Sai password."
                });
            }

            var role = user.IdLoaiTaiKhoan == 1 ? "Admin" : "User";
            var token = _jwtService.GenerateToken(user.Id.ToString(), role);

            return Ok(new ApiRespone
            {
                Success = true,
                Message = "Đăng nhập thành công!",
                Data = new
                {
                    user.Id,
                    user.HoTen,
                    user.Email,
                    Role = role,
                    Token = token
                }

            });


        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // Xóa cookie trên trình duyệt
            Response.Cookies.Delete("AccessToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/"
            });

            return Ok(new { message = "Logged out" });
        }

        // send Otp
        [HttpPost("SendOtp")]
        public async Task<IActionResult> SendOtp(string email)
        {
            var user = _context.TaiKhoans.FirstOrDefault(u => u.Email == email);
            if (user == null)
                return BadRequest(new ApiRespone
                {
                    Success = false,
                    Message = "Email không tồn tại!"
                });


            var otp = new Random().Next(100000, 999999).ToString();
            await _publicService.SendEmailAsync(email, "OTP Reset Password", $"Mã OTP của bạn là: {otp} . Mã có hiệu lực trong 5 phút.");

            user.Otp = otp;
            user.OtpExpire = DateTime.UtcNow.AddMinutes(5);
            _context.TaiKhoans.Update(user);
            await _context.SaveChangesAsync();
            return Ok(new ApiRespone
            {
                Success = true,
                Message = "Đã gửi OTP đến email của bạn!"
            });
        }


        // đặt lại mật khẩu
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] DatlaiMatKhau request)
        {
            var user = await _context.TaiKhoans.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
                return BadRequest(new ApiRespone
                {
                    Success = false,
                    Message = "Email không tồn tại!"
                });


            if (DateTime.UtcNow > user.OtpExpire)
                return BadRequest(new ApiRespone
                {
                    Success = false,
                    Message = "OTP đã hết hạn!"
                });

            if (user.Otp != request.Otp)
                return BadRequest(new ApiRespone
                {
                    Success = false,
                    Message = "OTP không đúng!"
                });

            // Hash lại mật khẩu mới
            var hashedPassword = _publicService.HashPassword(request.NewPassword);

            user.MatKhau = hashedPassword;
            user.Otp = null;
            user.OtpExpire = null;

            _context.TaiKhoans.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new ApiRespone
            {
                Success = true,
                Message = "Mật khẩu đã được đặt lại thành công!",

            });


        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var item = _context.TaiKhoans.ToList();
            return Ok(new ApiRespone
            {
                Success = true,
                Message = "các tài khoản",
                Data = item
            });

        }
    }
}
