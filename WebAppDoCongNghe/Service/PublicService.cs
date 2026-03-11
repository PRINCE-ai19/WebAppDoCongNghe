using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Runtime;
using WebAppDoCongNghe.Models.ApiRespone;
using WebAppDoCongNghe.Models.Entities;

namespace WebAppDoCongNghe.Service
{
    public class PublicService
    {
        private readonly EmailSettings _settings;

        private readonly AppDbContext _context;
        public PublicService(IOptions<EmailSettings> options , AppDbContext context)
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


        // g?i mail mã Otp
        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var message = new MailMessage();
            message.From = new MailAddress(_settings.From, _settings.DisplayName);
            message.To.Add(new MailAddress(toEmail));
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = false; // n?u mu?n d? html thì true

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


        // ? Xem chi ti?t thông báo
        public async Task<dynamic> GetDetail(int id)
        {
            var thongBao = await _context.ThongBaos.FindAsync(id);

            if (thongBao == null)
            {
                return new
                {
                    success = false,
                    message = "Không tìm th?y thông báo."
                };
            }

            // Ðánh d?u dã xem n?u chua xem
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


        // ? Thêm thông báo m?i
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
                message = "Ðã thêm thông báo m?i thành công.",
                data = thongBao
            };
        }


        // ? Xóa thông báo theo ID
        public async Task<dynamic> DeleteThongBao(int id)
        {
            var thongBao = await _context.ThongBaos.FindAsync(id);

            if (thongBao == null)
            {
                return new
                {
                    success = false,
                    message = "Không tìm th?y thông báo c?n xóa."
                };
            }

            _context.ThongBaos.Remove(thongBao);
            await _context.SaveChangesAsync();

            return new
            {
                success = true,
                message = "Ðã xóa thông báo thành công."
            };
        }

    }
}
