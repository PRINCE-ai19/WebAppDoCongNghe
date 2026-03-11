
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppDoCongNghe.Models.ApiRespone;
using WebAppDoCongNghe.Models.Entities;
using WebAppDoCongNghe.Models.model;
using WebAppDoCongNghe.Service;

namespace WebAppDoCongNghe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DangNhapController : ControllerBase
    {
        private readonly AppDbContext _context;

        private readonly PublicService _publicService;

        private readonly JwtService _jwtService;

        public DangNhapController(AppDbContext context , PublicService publicService , JwtService jwtService)
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
                    Message = "d? li?u nh?p vào dã sai ki?m tra l?i"
                });
            }

            //  Tìm tài kho?n trong database
            var user = _context.TaiKhoans.FirstOrDefault(u => u.Email == request.TaiKhoan || u.SoDienThoai == request.TaiKhoan);
            if (user == null)
            {
                return NotFound(new ApiRespone
                {
                    Success = false,
                    Message = "Sai email ."
                });
            }

            // so sánh mã hóa m?t kh?u 
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
                Message = "Ðang nh?p thành công!",
                Data = new
                {
                    user.Id,
                    user.HoTen,
                    user.Email,
                    user.HinhAnh,
                    Role = role,
                    Token = token
                }

            });


        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // Xóa cookie trên trình duy?t
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
        public async Task<IActionResult> SendOtp([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(new ApiRespone
                {
                    Success = false,
                    Message = "Email không du?c d? tr?ng!"
                });
            }

            var user = _context.TaiKhoans.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                return BadRequest(new ApiRespone
                {
                    Success = false,
                    Message = "Email không t?n t?i!"
                });
            }

            try
            {
                var otp = new Random().Next(100000, 999999).ToString();
                await _publicService.SendEmailAsync(email, "OTP Reset Password", $"Mã OTP c?a b?n là: {otp}. Mã có hi?u l?c trong 5 phút.");

                user.Otp = otp;
                user.OtpExpire = DateTime.UtcNow.AddMinutes(5);
                _context.TaiKhoans.Update(user);
                await _context.SaveChangesAsync();

                return Ok(new ApiRespone
                {
                    Success = true,
                    Message = "Ðã g?i OTP d?n email c?a b?n!"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiRespone
                {
                    Success = false,
                    Message = $"Không th? g?i email OTP: {ex.Message}"
                });
            }
        }


        // d?t l?i m?t kh?u
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] DatlaiMatKhau request)
        {
            var user = await _context.TaiKhoans.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
                return BadRequest(new ApiRespone
                {
                    Success = false,
                    Message = "Email không t?n t?i!"
                });


            if (DateTime.UtcNow > user.OtpExpire)
                return BadRequest(new ApiRespone
                {
                    Success = false,
                    Message = "OTP dã h?t h?n!"
                });

            if (user.Otp != request.Otp)
                return BadRequest(new ApiRespone
                {
                    Success = false,
                    Message = "OTP không dúng!"
                });

            // Hash l?i m?t kh?u m?i
            var hashedPassword = _publicService.HashPassword(request.NewPassword);

            user.MatKhau = hashedPassword;
            user.Otp = null;
            user.OtpExpire = null;

            _context.TaiKhoans.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new ApiRespone
            {
                Success = true,
                Message = "M?t kh?u dã du?c d?t l?i thành công!",

            });


        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var item = _context.TaiKhoans.ToList();
            return Ok(new ApiRespone
            {
                Success = true,
                Message = "các tài kho?n",
                Data = item
            });

        }
    }
}
