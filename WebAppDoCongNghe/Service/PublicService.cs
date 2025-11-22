using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Runtime;
using WebAppDoCongNghe.Models.ApiRespone;
using WebAppDoCongNghe.Models.Entity;

namespace WebAppDoCongNghe.Service
{
    public class PublicService
    {
        private readonly EmailSettings _settings;

        private readonly WebAppDoCongNgheContext _context;
        public PublicService(IOptions<EmailSettings> options , WebAppDoCongNgheContext context)
        {
            _settings = options.Value;
            _context = context;
        }

        // mã hóa password
        private readonly PasswordHasher<string> passwordHasher = new PasswordHasher<string>();
        public string HashPassword(string password)
        {
            return passwordHasher.HashPassword(null, password);
        }
        public PasswordVerificationResult PasswordVerification(string hashpassword, string providedpassword)
        {
            return passwordHasher.VerifyHashedPassword(null, hashpassword, providedpassword);
        }


        // gửi mail mã Otp
        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var message = new MailMessage();
            message.From = new MailAddress(_settings.From, _settings.DisplayName);
            message.To.Add(new MailAddress(toEmail));
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = false;

            using (var client = new SmtpClient(_settings.Host, _settings.Port))
            {
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(_settings.From, _settings.AppPassword);
                client.EnableSsl = true;

                await client.SendMailAsync(message);
            }
        }


        public async Task<dynamic> GetByUser(int taiKhoanId)
        {
            var list = await _context.ThongBaos
                .Where(tb => tb.TaiKhoanId == taiKhoanId).Select(r => new
                {
                    r.Id,
                    r.TaiKhoanId,
                    r.DaXem,
                    r.TieuDe,
                    r.NoiDung,
                    r.NgayTao

                })
                .OrderByDescending(tb => tb.NgayTao)
                .ToListAsync();

            if (list == null || list.Count == 0)
            {
                return new
                {
                    success = false,
                    message = "Không có thông báo nào."
                };
            }

            return new
            {
                success = true,
                data = list
            };
        }


        // ✅ Xem chi tiết thông báo
        public async Task<dynamic> GetDetail(int id)
        {
            var thongBao = await _context.ThongBaos.FindAsync(id);

            if (thongBao == null)
            {
                return new
                {
                    success = false,
                    message = "Không tìm thấy thông báo."
                };
            }

            // Đánh dấu đã xem nếu chưa xem
            if (thongBao.DaXem == false)
            {
                thongBao.DaXem = true;
                _context.ThongBaos.Update(thongBao);
                await _context.SaveChangesAsync();
            }

            return new
            {
                success = true,
                data = new
                {
                    thongBao.Id,
                    thongBao.TaiKhoanId,
                    thongBao.TieuDe,
                    thongBao.NoiDung,
                    thongBao.NgayTao,
                    thongBao.DaXem
                }
            };
        }


        // ✅ Thêm thông báo mới
        public async Task<dynamic> AddThongBao(int taiKhoanId, string tieuDe, string noiDung)
        {
            var thongBao = new ThongBao
            {
                TaiKhoanId = taiKhoanId,
                TieuDe = tieuDe,
                NoiDung = noiDung,
                NgayTao = DateTime.Now,
                DaXem = false
            };

            _context.ThongBaos.Add(thongBao);
            await _context.SaveChangesAsync();

            return new
            {
                success = true,
                message = "Đã thêm thông báo mới thành công.",
                data = thongBao
            };
        }


        // ✅ Xóa thông báo theo ID
        public async Task<dynamic> DeleteThongBao(int id)
        {
            var thongBao = await _context.ThongBaos.FindAsync(id);

            if (thongBao == null)
            {
                return new
                {
                    success = false,
                    message = "Không tìm thấy thông báo cần xóa."
                };
            }

            _context.ThongBaos.Remove(thongBao);
            await _context.SaveChangesAsync();

            return new
            {
                success = true,
                message = "Đã xóa thông báo thành công."
            };
        }

    }
}
