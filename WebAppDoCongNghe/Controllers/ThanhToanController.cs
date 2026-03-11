using BanDoCongNghe.Services.VnpayServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QuanLyDatVeMayBay.Services.VnpayServices.Enums;
using VNPAY.NET.Models;
using VNPAY.NET.Utilities;
using WebAppDoCongNghe.Models.ApiRespone;
using WebAppDoCongNghe.Models.Entities;
using WebAppDoCongNghe.Models.model;
using WebAppDoCongNghe.Service;

namespace WebAppDoCongNghe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ThanhToanController : ControllerBase
    {
        private readonly AppDbContext _context;

        private readonly IVnpay _vpnpay;

        private readonly VNPayConfig _config;

        private readonly IHubContext<NotificationHub> _hubContext;
        public ThanhToanController(IVnpay vnpay, IOptions<VNPayConfig> config, IHubContext<NotificationHub> hubContext , AppDbContext context)
        {

            _context = context;
            _vpnpay = vnpay;
            _config = config.Value;
            _vpnpay.Initialize(
                _config.vnp_TmnCode,
                _config.vnp_HashSecret,
                 _config.vnp_ReturnUrl,
                _config.vnp_Url

            );
            _hubContext = hubContext;
        }

        // Helper method d? tính giá gi?m t? khuy?n mãi
        private decimal TinhGiaGiamTuKhuyenMai(decimal giaGoc, int sanPhamId)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            
            // L?y khuy?n mãi dang active cho s?n ph?m này
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
                // Tính giá gi?m: giá g?c * (1 - ph?n tram gi?m / 100)
                return giaGoc * (1 - khuyenMaiActive / 100);
            }

            // N?u không có khuy?n mãi, tr? v? giá g?c
            return giaGoc;
        }

        [HttpGet("ThanhToan/Xem/{taiKhoanId}")]
        public IActionResult XemSanPhamThanhToan(int taiKhoanId)
        {
            //  L?y tài kho?n
            var taiKhoan = _context.TaiKhoans
                .FirstOrDefault(t => t.Id == taiKhoanId);

            if (taiKhoan == null)
                return NotFound(new { success = false, message = "Không tìm th?y tài kho?n." });

            //  L?y gi? hàng c?a tài kho?n
            var gioHang = _context.GioHangs.FirstOrDefault(g => g.TaiKhoanId == taiKhoanId);
            if (gioHang == null)
                return Ok(new { success = false, message = "Gi? hàng tr?ng." });

            //  L?y chi ti?t gi? hàng
            var chiTietList = _context.ChiTietGioHangs
                .Where(c => c.GioHangId == gioHang.Id)
                .ToList();

            if (!chiTietList.Any())
                return Ok(new { success = false, message = "Gi? hàng tr?ng." });

            //  L?y danh sách s?n ph?m liên quan
            var sanPhamIds = chiTietList.Select(c => c.SanPhamId).ToList();
            var sanPhamDict = _context.SanPhams
                .Where(sp => sanPhamIds.Contains(sp.Id))
                .ToDictionary(sp => sp.Id, sp => sp);

            //  L?y hình ?nh d?u tiên c?a m?i s?n ph?m
            var hinhAnhDict = _context.HinhAnhSanPhams
                .Where(h => sanPhamIds.Contains(h.SanPhamId))
                .GroupBy(h => h.SanPhamId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.HinhAnh).FirstOrDefault());

            //  G?p d? li?u và tính giá gi?m t? khuy?n mãi
            var data = chiTietList.Select(c =>
            {
                var sanPhamId = c.SanPhamId.GetValueOrDefault();
                var sp = sanPhamDict[sanPhamId];
                var anh = hinhAnhDict.ContainsKey(sanPhamId) ? hinhAnhDict[sanPhamId] : null;
                
                // Tính giá gi?m t? khuy?n mãi
                var giaGoc = sp.Gia;
                var giaGiamTuKhuyenMai = TinhGiaGiamTuKhuyenMai(giaGoc, sanPhamId);
                
                // Uu tiên giá gi?m t? khuy?n mãi, n?u không có thì dùng giá gi?m cu ho?c giá g?c
                var giaCuoiCung = giaGiamTuKhuyenMai < giaGoc ? giaGiamTuKhuyenMai : (sp.GiaGiam ?? giaGoc);
                var thanhTien = giaCuoiCung * c.SoLuong;

                return new
                {
                    SanPhamId = c.SanPhamId,
                    TenSanPham = sp.TenSanPham,
                    Gia = giaGoc,
                    GiaGiam = giaCuoiCung,
                    SoLuong = c.SoLuong,
                    ThanhTien = thanhTien,
                    AnhDaiDien = anh
                };
            }).ToList();

            var tongTien = data.Sum(x => x.ThanhTien);

            //  L?y danh sách voucher dã claim và chua s? d?ng c?a user
            var now = DateTime.Now;
            var vouchers = _context.TaiKhoanPhieuGiamGia
                .Where(uv => uv.TaiKhoanId == taiKhoanId && uv.DaSuDung == false)
                .Include(uv => uv.PhieuGiamGia)
                .Select(uv => uv.PhieuGiamGia)
                .Where(v => v.TrangThai == true 
                         && v.NgayBatDau <= now 
                         && v.NgayKetThuc >= now
                         && v.SoLuong > 0)
                .Select(v => new
                {
                    Id = v.Id,
                    MaPhieu = v.MaPhieu,
                    MoTa = v.MoTa,
                    GiaTriGiam = v.GiaTriGiam,
                    KieuGiam = v.KieuGiam,
                    NgayKetThuc = v.NgayKetThuc
                })
                .ToList();

            //  Tr? v? k?t qu? có thông tin user và voucher
            return Ok(new
            {
                success = true,
                message = "L?y danh sách s?n ph?m thanh toán thành công.",
                tongTien,
                khachHang = new
                {
                    HoTen = taiKhoan.HoTen,
                    Email = taiKhoan.Email,
                    SoDienThoai = taiKhoan.SoDienThoai,
                    DiaChi = taiKhoan.DiaChi
                },
                vouchers = vouchers, // Danh sách voucher có th? s? d?ng
                data
            });
        }
        [HttpPost("TestVNPAY")]
        public IActionResult Test()
        {
            var ipAddress = NetworkHelper.GetIpAddress(HttpContext);
            var state = "HOANGLUAN";
            var request = new PaymentRequest
            {
                PaymentId = DateTime.Now.Ticks,
                Money = (double)50000,
                Description = "Thanh toán s?n ph?m!",
                IpAddress = ipAddress,
                CreatedDate = DateTime.Now,
                Currency = Currency.VND,
                Language = DisplayLanguage.Vietnamese
            };
            var paymentUrl = _vpnpay.GetPaymentUrl(request);
            return Ok(new
            {
                statusCode = 201,
                message = "Ðang chuy?n d?n trang thanh toán VNPay...",
                url = paymentUrl,
                state = state
            });
        }

        [HttpPost("ThanhToan/DatHang")]
        public IActionResult DatHang([FromBody] DatHangRequest model)
        {
            if (model == null)
                return BadRequest(new { success = false, message = "D? li?u không h?p l?." });

            var gioHang = _context.GioHangs.FirstOrDefault(g => g.TaiKhoanId == model.TaiKhoanId);
            if (gioHang == null)
                return BadRequest(new { success = false, message = "Không tìm th?y gi? hàng." });

            var chiTietGioHang = _context.ChiTietGioHangs
                                         .Where(c => c.GioHangId == gioHang.Id)
                                         .ToList();
            if (chiTietGioHang.Count == 0)
                return BadRequest(new { success = false, message = "Gi? hàng tr?ng." });

            var sanPhamIds = chiTietGioHang.Select(c => c.SanPhamId).ToList();
            var sanPhamDict = _context.SanPhams
                .Where(sp => sanPhamIds.Contains(sp.Id))
                .ToDictionary(sp => sp.Id, sp => sp);

            decimal? tongTien = 0;

            foreach (var item in chiTietGioHang)
            {
                if (sanPhamDict.TryGetValue(item.SanPhamId.GetValueOrDefault(), out var sanPham))
                {
                    // Tính giá gi?m t? khuy?n mãi
                    var giaGoc = sanPham.Gia;
                    var giaGiamTuKhuyenMai = TinhGiaGiamTuKhuyenMai(giaGoc, sanPham.Id);
                    
                    // Uu tiên giá gi?m t? khuy?n mãi, n?u không có thì dùng giá gi?m cu ho?c giá g?c
                    var giaCuoiCung = giaGiamTuKhuyenMai < giaGoc ? giaGiamTuKhuyenMai : (sanPham.GiaGiam ?? giaGoc);
                    
                    tongTien += giaCuoiCung * item.SoLuong;
                }
            }

            //  X? lý mã gi?m giá (n?u có)
            decimal? tienGiam = 0;
            int? phieuGiamGiaId = null;
            TaiKhoanPhieuGiamGium? userVoucher = null;

            if (!string.IsNullOrWhiteSpace(model.MaPhieuGiamGia))
            {
                // Tìm phi?u gi?m giá theo mã
                var voucher = _context.PhieuGiamGia
                    .FirstOrDefault(v => v.MaPhieu == model.MaPhieuGiamGia.Trim());

                if (voucher == null)
                {
                    return BadRequest(new { success = false, message = "Mã gi?m giá không t?n t?i." });
                }

                // Ki?m tra voucher còn h?n
                var now = DateTime.Now;
                if (now < voucher.NgayBatDau || now > voucher.NgayKetThuc)
                {
                    return BadRequest(new { success = false, message = "Mã gi?m giá dã h?t h?n s? d?ng." });
                }

                // Ki?m tra voucher còn s? lu?ng
                if (voucher.SoLuong <= 0)
                {
                    return BadRequest(new { success = false, message = "Mã gi?m giá dã h?t s? lu?ng." });
                }

                // Ki?m tra tr?ng thái voucher
                if (voucher.TrangThai != true)
                {
                    return BadRequest(new { success = false, message = "Mã gi?m giá dang t?m ngung." });
                }

                // Ki?m tra user dã claim voucher chua
                userVoucher = _context.TaiKhoanPhieuGiamGia
                    .FirstOrDefault(uv => uv.TaiKhoanId == model.TaiKhoanId 
                                      && uv.PhieuGiamGiaId == voucher.Id);

                if (userVoucher == null)
                {
                    return BadRequest(new { success = false, message = "B?n chua nh?n mã gi?m giá này. Vui lòng nh?n mã tru?c khi s? d?ng." });
                }

                // Ki?m tra voucher dã du?c dùng chua
                if (userVoucher.DaSuDung == true)
                {
                    return BadRequest(new { success = false, message = "Mã gi?m giá này dã du?c s? d?ng." });
                }

                // Tính toán ti?n gi?m
                if (voucher.KieuGiam == "percentage")
                {
                    // Gi?m theo ph?n tram (t?i da 99%)
                    var phanTram = Math.Min((double)voucher.GiaTriGiam, 99);
                    tienGiam = tongTien * (decimal)(phanTram / 100);
                }
                else
                {
                    // Gi?m theo s? ti?n c? d?nh
                    tienGiam = voucher.GiaTriGiam;
                    // Ð?m b?o không gi?m quá t?ng ti?n
                    if (tienGiam > tongTien)
                    {
                        tienGiam = tongTien;
                    }
                }

                phieuGiamGiaId = voucher.Id;
            }

            // Tính t?ng ti?n sau gi?m giá
            var tongTienSauGiam = tongTien - tienGiam;
            if (tongTienSauGiam < 0) tongTienSauGiam = 0;

            //  N?u thanh toán VNPay, ch? t?o ThanhToanTam t?m th?i, chua t?o don hàng
            if (model.payment == 1)
            {
              
                var orderInfo = new
                {
                    TaiKhoanId = model.TaiKhoanId,
                    TongTien = tongTienSauGiam,
                    DiaChiGiao = model.DiaChiGiao ?? string.Empty,
                    GhiChu = model.GhiChu ?? string.Empty,
                    PhieuGiamGiaId = phieuGiamGiaId,
                    ChiTietGioHang = chiTietGioHang.Select(c => new
                    {
                        SanPhamId = c.SanPhamId,
                        SoLuong = c.SoLuong
                    }).ToList()
                };
                var orderInfoJson = System.Text.Json.JsonSerializer.Serialize(orderInfo);

                var thanhToanTam = new ThanhToanTam
                {
                    DonHangId = null, // Chua có don hàng
                    TongTien = tongTienSauGiam.Value,
                    IsVnPay = false,
                    TrangThai = "Ch? thanh toán VNPay",
                    NgayTao = DateTime.Now,
                    TaiKhoanId = model.TaiKhoanId,
                    NoiDung = orderInfoJson // Luu thông tin don hàng t?m th?i
                };
                _context.ThanhToanTams.Add(thanhToanTam);
                _context.SaveChanges();

                var ipAddress = NetworkHelper.GetIpAddress(HttpContext);
                var state = "HOANGLUAN";
                var request = new PaymentRequest
                {
                    PaymentId = thanhToanTam.Id,
                    Money = (double)tongTienSauGiam,
                    Description = "Thanh toán s?n ph?m!",
                    IpAddress = ipAddress,
                    CreatedDate = DateTime.Now,
                    Currency = Currency.VND,
                    Language = DisplayLanguage.Vietnamese
                };
                var paymentUrl = _vpnpay.GetPaymentUrl(request);
                return Ok(new
                {
                    statusCode = 201,
                    message = "Ðang chuy?n d?n trang thanh toán VNPay...",
                    url = paymentUrl,
                    state = state
                });
            }

            // N?u thanh toán COD, t?o don hàng ngay
            var donHang = new DonHang
            {
                TaiKhoanId = model.TaiKhoanId,
                NgayDat = DateTime.Now,
                TongTien = tongTienSauGiam,
                TrangThai = OrderStatus.ChoXuLy,
                DiaChiGiao = model.DiaChiGiao ?? string.Empty,
                GhiChu = model.GhiChu ?? string.Empty,
                PhuongThucThanhToan = true,
                PhieuGiamGiaId = phieuGiamGiaId
            };

            _context.DonHangs.Add(donHang);
            _context.SaveChanges();

            // C?p nh?t tr?ng thái voucher dã s? d?ng (n?u có)
            if (userVoucher != null && phieuGiamGiaId.HasValue)
            {
                userVoucher.DaSuDung = true;
                userVoucher.NgaySuDung = DateTime.Now;
                _context.TaiKhoanPhieuGiamGia.Update(userVoucher);
            }

            foreach (var item in chiTietGioHang)
            {
                if (sanPhamDict.TryGetValue(item.SanPhamId.GetValueOrDefault(), out var sanPham))
                {
                    // Tính giá gi?m t? khuy?n mãi d? luu vào don hàng
                    var giaGoc = sanPham.Gia;
                    var giaGiamTuKhuyenMai = TinhGiaGiamTuKhuyenMai(giaGoc, sanPham.Id);
                    
                    // Uu tiên giá gi?m t? khuy?n mãi, n?u không có thì dùng giá gi?m cu ho?c giá g?c
                    var giaCuoiCung = giaGiamTuKhuyenMai < giaGoc ? giaGiamTuKhuyenMai : (sanPham.GiaGiam ?? giaGoc);
                    
                    _context.ChiTietDonHangs.Add(new ChiTietDonHang
                    {
                        DonHangId = donHang.Id,
                        SanPhamId = item.SanPhamId,
                        SoLuong = item.SoLuong,
                        DonGia = giaCuoiCung // Luu giá gi?m vào don hàng
                    });
                    if (item.SoLuong > sanPham.SoLuongTon)
                    {
                        return Ok(new
                        {
                            success = false,
                            message = "có v?n d? xáy ra khi luu"
                        });
                    }
                    sanPham.SoLuongTon = sanPham.SoLuongTon - item.SoLuong;
                }
            }

            var thanhToan = new ThanhToanTam
            {
                DonHangId = donHang.Id,
                TongTien = tongTienSauGiam.Value,
                IsVnPay = false,
                TrangThai = "Chua thanh toán",
                NgayTao = DateTime.Now,
                TaiKhoanId = model.TaiKhoanId,
            };
            _context.ThanhToanTams.Add(thanhToan);

            var giaoHang = new GiaoHang
            {
                DonHangId = donHang.Id,
                TrangThai = "Ðang chu?n b? hàng",
                NgayCapNhat = DateTime.Now,
                DonViVanChuyen = "Chua xác d?nh"
            };
            _context.GiaoHangs.Add(giaoHang);

            var thongBaoNoiDung = phieuGiamGiaId.HasValue
                ? $"B?n dã d?t don hàng #{donHang.Id} v?i t?ng ti?n {tongTienSauGiam:N0} VND (dã gi?m {tienGiam:N0} VND). Ðon hàng dang du?c x? lý."
                : $"B?n dã d?t don hàng #{donHang.Id} v?i t?ng ti?n {tongTienSauGiam:N0} VND. Ðon hàng dang du?c x? lý.";

            var thongBao = new ThongBao
            {
                TaiKhoanId = model.TaiKhoanId,
                TieuDe = "Ð?t hàng thành công",
                NoiDung = thongBaoNoiDung,
                NgayTao = DateTime.Now,
                DaXem = false
            };
            _context.ThongBaos.Add(thongBao);
            _context.ChiTietGioHangs.RemoveRange(chiTietGioHang);

            _context.SaveChanges();

            return Ok(new
            {
                success = true,
                message = "Ð?t hàng thành công!",
            });
        }

        [HttpGet("ReturnVnPay")]
        public async Task<IActionResult> ReturnVnPay()
        {
            if (Request.QueryString.HasValue)
            {
                try
                {
                    var paymentResult = _vpnpay.GetPaymentResult(Request.Query);
                    var ThanhToan = await _context.ThanhToanTams.FindAsync((int)paymentResult.PaymentId);
                    if (ThanhToan == null)
                    {
                        return BadRequest(new
                        {
                            message = "Không tìm th?y thông tin thanh toán"
                        });
                    }

                    // ?? Ki?m tra n?u don hàng chua du?c t?o (DonHangId == null) thì t?o don hàng
                    if (ThanhToan.DonHangId == null && !string.IsNullOrEmpty(ThanhToan.NoiDung))
                    {
                        // Parse thông tin don hàng t? NoiDung (JSON)
                        var orderInfo = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(ThanhToan.NoiDung);
                        
                        var taiKhoanId = orderInfo.GetProperty("TaiKhoanId").GetInt32();
                        var tongTien = orderInfo.GetProperty("TongTien").GetDecimal();
                        var diaChiGiao = orderInfo.GetProperty("DiaChiGiao").GetString() ?? string.Empty;
                        var ghiChu = orderInfo.GetProperty("GhiChu").GetString() ?? string.Empty;
                        var phieuGiamGiaId = orderInfo.TryGetProperty("PhieuGiamGiaId", out var pgg) && pgg.ValueKind != System.Text.Json.JsonValueKind.Null 
                            ? pgg.GetInt32() 
                            : (int?)null;
                        var chiTietGioHang = orderInfo.GetProperty("ChiTietGioHang").EnumerateArray().ToList();

                        // L?y gi? hàng
                        var gioHang = _context.GioHangs.FirstOrDefault(g => g.TaiKhoanId == taiKhoanId);
                        if (gioHang == null)
                        {
                            return BadRequest(new { message = "Không tìm th?y gi? hàng." });
                        }

                        // T?o don hàng
                        var donHang = new DonHang
                        {
                            TaiKhoanId = taiKhoanId,
                            NgayDat = DateTime.Now,
                            TongTien = tongTien,
                            TrangThai = "Ch? xác nh?n",
                            DiaChiGiao = diaChiGiao,
                            GhiChu = ghiChu,
                            PhuongThucThanhToan = true,
                            PhieuGiamGiaId = phieuGiamGiaId
                        };
                        _context.DonHangs.Add(donHang);
                        await _context.SaveChangesAsync();

                        // L?y thông tin s?n ph?m
                        var sanPhamIds = chiTietGioHang.Select(c => c.GetProperty("SanPhamId").GetInt32()).ToList();
                        var sanPhamDict = _context.SanPhams
                            .Where(sp => sanPhamIds.Contains(sp.Id))
                            .ToDictionary(sp => sp.Id, sp => sp);

                        // T?o chi ti?t don hàng và tr? s? lu?ng t?n kho
                        foreach (var item in chiTietGioHang)
                        {
                            var sanPhamId = item.GetProperty("SanPhamId").GetInt32();
                            var soLuong = item.GetProperty("SoLuong").GetInt32();

                            if (sanPhamDict.TryGetValue(sanPhamId, out var sanPham))
                            {
                                // Tính giá gi?m t? khuy?n mãi d? luu vào don hàng
                                var giaGoc = sanPham.Gia;
                                var giaGiamTuKhuyenMai = TinhGiaGiamTuKhuyenMai(giaGoc, sanPham.Id);
                                
                                // Uu tiên giá gi?m t? khuy?n mãi, n?u không có thì dùng giá gi?m cu ho?c giá g?c
                                var giaCuoiCung = giaGiamTuKhuyenMai < giaGoc ? giaGiamTuKhuyenMai : (sanPham.GiaGiam ?? giaGoc);
                                
                                _context.ChiTietDonHangs.Add(new ChiTietDonHang
                                {
                                    DonHangId = donHang.Id,
                                    SanPhamId = sanPhamId,
                                    SoLuong = soLuong,
                                    DonGia = giaCuoiCung // Luu giá gi?m vào don hàng
                                });

                                if (soLuong > sanPham.SoLuongTon)
                                {
                                    return BadRequest(new { message = "S?n ph?m không d? s? lu?ng trong kho." });
                                }
                                sanPham.SoLuongTon = sanPham.SoLuongTon - soLuong;
                            }
                        }

                        // C?p nh?t voucher dã s? d?ng (n?u có)
                        if (phieuGiamGiaId.HasValue)
                        {
                            var userVoucher = _context.TaiKhoanPhieuGiamGia
                                .FirstOrDefault(uv => uv.TaiKhoanId == taiKhoanId && uv.PhieuGiamGiaId == phieuGiamGiaId.Value);
                            if (userVoucher != null && userVoucher.DaSuDung != true)
                            {
                                userVoucher.DaSuDung = true;
                                userVoucher.NgaySuDung = DateTime.Now;
                                _context.TaiKhoanPhieuGiamGia.Update(userVoucher);
                            }
                        }

                        // Xóa gi? hàng
                        var chiTietGioHangList = _context.ChiTietGioHangs
                            .Where(c => c.GioHangId == gioHang.Id)
                            .ToList();
                        _context.ChiTietGioHangs.RemoveRange(chiTietGioHangList);

                        // T?o giao hàng
                        var giaoHang = new GiaoHang
                        {
                            DonHangId = donHang.Id,
                            TrangThai = "Ðang chu?n b? hàng",
                            NgayCapNhat = DateTime.Now,
                            DonViVanChuyen = "Chua xác d?nh"
                        };
                        _context.GiaoHangs.Add(giaoHang);

                        // C?p nh?t ThanhToanTam v?i DonHangId
                        ThanhToan.DonHangId = donHang.Id;
                    }

                    // C?p nh?t tr?ng thái thanh toán
                    ThanhToan.TrangThai = "dã chuy?n kho?n";
                    ThanhToan.IsVnPay = true;
                    _context.ThanhToanTams.Update(ThanhToan);

                    // T?o thông báo
                    var thongbao = new ThongBao
                    {
                        TaiKhoanId = ThanhToan.TaiKhoanId,
                        TieuDe = "Ð?t hàng thành công",
                        NoiDung = $"B?n dã d?t don hàng #{ThanhToan.DonHangId} v?i t?ng ti?n {ThanhToan.TongTien:N0} VND v?i phuong th?c thanh toán là VNPAY. Ðon hàng dang du?c x? lý.",
                        NgayTao = DateTime.Now,
                        DaXem = false
                    };
                    _context.Add(thongbao);
                    await _context.SaveChangesAsync();



                    var state = "HOANGLUAN";

                    try
                    {
                        if (!string.IsNullOrEmpty(state))
                        {
                            await _hubContext.Clients.Group(state).SendAsync("payment", new
                            {
                                statusCode = 200,
                                message = "Ð?t hàng thành công!",
                            });

                            Console.WriteLine($"?? Ðã g?i thông báo SignalR t?i group: {state}");
                        }
                        else
                        {
                            Console.WriteLine("?? State tr?ng, không g?i du?c thông báo SignalR");
                        }
                    }
                    catch (Exception hubEx)
                    {
                        Console.WriteLine("? L?i khi g?i SignalR: " + hubEx.Message);
                        if (hubEx.InnerException != null)
                            Console.WriteLine("? Inner: " + hubEx.InnerException.Message);
                    }


                    string html = @"
<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Thanh toán thành công</title>
    <style>
        body {
            background-color: #f0fdf4;
            font-family: Arial, sans-serif;
            display: flex;
            justify-content: center;
            align-items: center;
            height: 100vh;
            margin: 0;
        }
        .container {
            background-color: #fff;
            text-align: center;
            padding: 40px;
            border-radius: 16px;
            box-shadow: 0 6px 25px rgba(0, 0, 0, 0.1);
            max-width: 400px;
        }
        .checkmark {
            color: #16a34a;
            font-size: 60px;
            margin-bottom: 20px;
        }
        h1 {
            color: #065f46;
            margin-bottom: 10px;
        }
        p {
            color: #4b5563;
            margin-bottom: 30px;
        }
        .countdown {
            color: #16a34a;
            font-weight: bold;
            margin-bottom: 20px;
        }
        .btn {
            display: inline-block;
            background-color: #16a34a;
            color: white;
            text-decoration: none;
            padding: 12px 20px;
            border-radius: 8px;
            font-weight: bold;
            transition: background 0.3s ease;
        }
        .btn:hover {
            background-color: #15803d;
        }
    </style>
    <script>
        let countdown = 5;
        const countdownElement = document.getElementById('countdown');
        
        function updateCountdown() {
            if (countdownElement) {
                countdownElement.textContent = 'T? d?ng chuy?n v? trang ch? sau ' + countdown + ' giây...';
            }
            countdown--;
            if (countdown < 0) {
                window.location.href = 'http://localhost:5173/';
            }
        }
        
        window.onload = function() {
            setInterval(updateCountdown, 1000);
        };
    </script>
</head>
<body>
    <div class='container'>
        <div class='checkmark'>?</div>
        <h1>Thanh toán thành công!</h1>
        <p>C?m on b?n dã mua hàng.<br>Ðon hàng c?a b?n dang du?c x? lý.</p>
        <p class='countdown' id='countdown'>T? d?ng chuy?n v? trang ch? sau 5 giây...</p>
        <a href='http://localhost:5173/' class='btn'>V? trang ch? ngay</a>
    </div>
</body>
</html>";

                    return Content(html, "text/html");

                }
                catch (Exception ex)
                {
                    return BadRequest(new { success = false, message = "L?i thanh toán." });
                }
            }

            return NotFound("có gì dó x?y ra r?i");
        }

    }
}

