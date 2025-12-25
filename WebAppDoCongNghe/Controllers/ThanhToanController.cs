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
using WebAppDoCongNghe.Models.Entity;
using WebAppDoCongNghe.Models.model;
using WebAppDoCongNghe.Service;

namespace WebAppDoCongNghe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ThanhToanController : ControllerBase
    {
        private readonly WebAppDoCongNgheContext _context;

        private readonly IVnpay _vpnpay;

        private readonly VNPayConfig _config;

        private readonly IHubContext<NotificationHub> _hubContext;
        public ThanhToanController(IVnpay vnpay, IOptions<VNPayConfig> config, IHubContext<NotificationHub> hubContext , WebAppDoCongNgheContext context)
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

        // Helper method để tính giá giảm từ khuyến mãi
        private decimal TinhGiaGiamTuKhuyenMai(decimal giaGoc, int sanPhamId)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            
            // Lấy khuyến mãi đang active cho sản phẩm này
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
                // Tính giá giảm: giá gốc * (1 - phần trăm giảm / 100)
                return giaGoc * (1 - khuyenMaiActive / 100);
            }

            // Nếu không có khuyến mãi, trả về giá gốc
            return giaGoc;
        }

        [HttpGet("ThanhToan/Xem/{taiKhoanId}")]
        public IActionResult XemSanPhamThanhToan(int taiKhoanId)
        {
            //  Lấy tài khoản
            var taiKhoan = _context.TaiKhoans
                .FirstOrDefault(t => t.Id == taiKhoanId);

            if (taiKhoan == null)
                return NotFound(new { success = false, message = "Không tìm thấy tài khoản." });

            //  Lấy giỏ hàng của tài khoản
            var gioHang = _context.GioHangs.FirstOrDefault(g => g.TaiKhoanId == taiKhoanId);
            if (gioHang == null)
                return Ok(new { success = false, message = "Giỏ hàng trống." });

            //  Lấy chi tiết giỏ hàng
            var chiTietList = _context.ChiTietGioHangs
                .Where(c => c.GioHangId == gioHang.Id)
                .ToList();

            if (!chiTietList.Any())
                return Ok(new { success = false, message = "Giỏ hàng trống." });

            //  Lấy danh sách sản phẩm liên quan
            var sanPhamIds = chiTietList.Select(c => c.SanPhamId).ToList();
            var sanPhamDict = _context.SanPhams
                .Where(sp => sanPhamIds.Contains(sp.Id))
                .ToDictionary(sp => sp.Id, sp => sp);

            //  Lấy hình ảnh đầu tiên của mỗi sản phẩm
            var hinhAnhDict = _context.HinhAnhSanPhams
                .Where(h => sanPhamIds.Contains(h.SanPhamId))
                .GroupBy(h => h.SanPhamId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.HinhAnh).FirstOrDefault());

            //  Gộp dữ liệu và tính giá giảm từ khuyến mãi
            var data = chiTietList.Select(c =>
            {
                var sanPhamId = c.SanPhamId.GetValueOrDefault();
                var sp = sanPhamDict[sanPhamId];
                var anh = hinhAnhDict.ContainsKey(sanPhamId) ? hinhAnhDict[sanPhamId] : null;
                
                // Tính giá giảm từ khuyến mãi
                var giaGoc = sp.Gia;
                var giaGiamTuKhuyenMai = TinhGiaGiamTuKhuyenMai(giaGoc, sanPhamId);
                
                // Ưu tiên giá giảm từ khuyến mãi, nếu không có thì dùng giá giảm cũ hoặc giá gốc
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

            //  Lấy danh sách voucher đã claim và chưa sử dụng của user
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

            //  Trả về kết quả có thông tin user và voucher
            return Ok(new
            {
                success = true,
                message = "Lấy danh sách sản phẩm thanh toán thành công.",
                tongTien,
                khachHang = new
                {
                    HoTen = taiKhoan.HoTen,
                    Email = taiKhoan.Email,
                    SoDienThoai = taiKhoan.SoDienThoai,
                    DiaChi = taiKhoan.DiaChi
                },
                vouchers = vouchers, // Danh sách voucher có thể sử dụng
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
                Description = "Thanh toán sản phẩm!",
                IpAddress = ipAddress,
                CreatedDate = DateTime.Now,
                Currency = Currency.VND,
                Language = DisplayLanguage.Vietnamese
            };
            var paymentUrl = _vpnpay.GetPaymentUrl(request);
            return Ok(new
            {
                statusCode = 201,
                message = "Đang chuyển đến trang thanh toán VNPay...",
                url = paymentUrl,
                state = state
            });
        }

        [HttpPost("ThanhToan/DatHang")]
        public IActionResult DatHang([FromBody] DatHangRequest model)
        {
            if (model == null)
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ." });

            var gioHang = _context.GioHangs.FirstOrDefault(g => g.TaiKhoanId == model.TaiKhoanId);
            if (gioHang == null)
                return BadRequest(new { success = false, message = "Không tìm thấy giỏ hàng." });

            var chiTietGioHang = _context.ChiTietGioHangs
                                         .Where(c => c.GioHangId == gioHang.Id)
                                         .ToList();
            if (chiTietGioHang.Count == 0)
                return BadRequest(new { success = false, message = "Giỏ hàng trống." });

            var sanPhamIds = chiTietGioHang.Select(c => c.SanPhamId).ToList();
            var sanPhamDict = _context.SanPhams
                .Where(sp => sanPhamIds.Contains(sp.Id))
                .ToDictionary(sp => sp.Id, sp => sp);

            decimal? tongTien = 0;

            foreach (var item in chiTietGioHang)
            {
                if (sanPhamDict.TryGetValue(item.SanPhamId.GetValueOrDefault(), out var sanPham))
                {
                    // Tính giá giảm từ khuyến mãi
                    var giaGoc = sanPham.Gia;
                    var giaGiamTuKhuyenMai = TinhGiaGiamTuKhuyenMai(giaGoc, sanPham.Id);
                    
                    // Ưu tiên giá giảm từ khuyến mãi, nếu không có thì dùng giá giảm cũ hoặc giá gốc
                    var giaCuoiCung = giaGiamTuKhuyenMai < giaGoc ? giaGiamTuKhuyenMai : (sanPham.GiaGiam ?? giaGoc);
                    
                    tongTien += giaCuoiCung * item.SoLuong;
                }
            }

            //  Xử lý mã giảm giá (nếu có)
            decimal? tienGiam = 0;
            int? phieuGiamGiaId = null;
            TaiKhoanPhieuGiamGium? userVoucher = null;

            if (!string.IsNullOrWhiteSpace(model.MaPhieuGiamGia))
            {
                // Tìm phiếu giảm giá theo mã
                var voucher = _context.PhieuGiamGia
                    .FirstOrDefault(v => v.MaPhieu == model.MaPhieuGiamGia.Trim());

                if (voucher == null)
                {
                    return BadRequest(new { success = false, message = "Mã giảm giá không tồn tại." });
                }

                // Kiểm tra voucher còn hạn
                var now = DateTime.Now;
                if (now < voucher.NgayBatDau || now > voucher.NgayKetThuc)
                {
                    return BadRequest(new { success = false, message = "Mã giảm giá đã hết hạn sử dụng." });
                }

                // Kiểm tra voucher còn số lượng
                if (voucher.SoLuong <= 0)
                {
                    return BadRequest(new { success = false, message = "Mã giảm giá đã hết số lượng." });
                }

                // Kiểm tra trạng thái voucher
                if (voucher.TrangThai != true)
                {
                    return BadRequest(new { success = false, message = "Mã giảm giá đang tạm ngưng." });
                }

                // Kiểm tra user đã claim voucher chưa
                userVoucher = _context.TaiKhoanPhieuGiamGia
                    .FirstOrDefault(uv => uv.TaiKhoanId == model.TaiKhoanId 
                                      && uv.PhieuGiamGiaId == voucher.Id);

                if (userVoucher == null)
                {
                    return BadRequest(new { success = false, message = "Bạn chưa nhận mã giảm giá này. Vui lòng nhận mã trước khi sử dụng." });
                }

                // Kiểm tra voucher đã được dùng chưa
                if (userVoucher.DaSuDung == true)
                {
                    return BadRequest(new { success = false, message = "Mã giảm giá này đã được sử dụng." });
                }

                // Tính toán tiền giảm
                if (voucher.KieuGiam == "percentage")
                {
                    // Giảm theo phần trăm (tối đa 99%)
                    var phanTram = Math.Min((double)voucher.GiaTriGiam, 99);
                    tienGiam = tongTien * (decimal)(phanTram / 100);
                }
                else
                {
                    // Giảm theo số tiền cố định
                    tienGiam = voucher.GiaTriGiam;
                    // Đảm bảo không giảm quá tổng tiền
                    if (tienGiam > tongTien)
                    {
                        tienGiam = tongTien;
                    }
                }

                phieuGiamGiaId = voucher.Id;
            }

            // Tính tổng tiền sau giảm giá
            var tongTienSauGiam = tongTien - tienGiam;
            if (tongTienSauGiam < 0) tongTienSauGiam = 0;

            //  Nếu thanh toán VNPay, chỉ tạo ThanhToanTam tạm thời, chưa tạo đơn hàng
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
                    DonHangId = null, // Chưa có đơn hàng
                    TongTien = tongTienSauGiam.Value,
                    IsVnPay = false,
                    TrangThai = "Chờ thanh toán VNPay",
                    NgayTao = DateTime.Now,
                    TaiKhoanId = model.TaiKhoanId,
                    NoiDung = orderInfoJson // Lưu thông tin đơn hàng tạm thời
                };
                _context.ThanhToanTams.Add(thanhToanTam);
                _context.SaveChanges();

                var ipAddress = NetworkHelper.GetIpAddress(HttpContext);
                var state = "HOANGLUAN";
                var request = new PaymentRequest
                {
                    PaymentId = thanhToanTam.Id,
                    Money = (double)tongTienSauGiam,
                    Description = "Thanh toán sản phẩm!",
                    IpAddress = ipAddress,
                    CreatedDate = DateTime.Now,
                    Currency = Currency.VND,
                    Language = DisplayLanguage.Vietnamese
                };
                var paymentUrl = _vpnpay.GetPaymentUrl(request);
                return Ok(new
                {
                    statusCode = 201,
                    message = "Đang chuyển đến trang thanh toán VNPay...",
                    url = paymentUrl,
                    state = state
                });
            }

            // Nếu thanh toán COD, tạo đơn hàng ngay
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

            // Cập nhật trạng thái voucher đã sử dụng (nếu có)
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
                    // Tính giá giảm từ khuyến mãi để lưu vào đơn hàng
                    var giaGoc = sanPham.Gia;
                    var giaGiamTuKhuyenMai = TinhGiaGiamTuKhuyenMai(giaGoc, sanPham.Id);
                    
                    // Ưu tiên giá giảm từ khuyến mãi, nếu không có thì dùng giá giảm cũ hoặc giá gốc
                    var giaCuoiCung = giaGiamTuKhuyenMai < giaGoc ? giaGiamTuKhuyenMai : (sanPham.GiaGiam ?? giaGoc);
                    
                    _context.ChiTietDonHangs.Add(new ChiTietDonHang
                    {
                        DonHangId = donHang.Id,
                        SanPhamId = item.SanPhamId,
                        SoLuong = item.SoLuong,
                        DonGia = giaCuoiCung // Lưu giá giảm vào đơn hàng
                    });
                    if (item.SoLuong > sanPham.SoLuongTon)
                    {
                        return Ok(new
                        {
                            success = false,
                            message = "có vấn đề xáy ra khi lưu"
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
                TrangThai = "Chưa thanh toán",
                NgayTao = DateTime.Now,
                TaiKhoanId = model.TaiKhoanId,
            };
            _context.ThanhToanTams.Add(thanhToan);

            var giaoHang = new GiaoHang
            {
                DonHangId = donHang.Id,
                TrangThai = "Đang chuẩn bị hàng",
                NgayCapNhat = DateTime.Now,
                DonViVanChuyen = "Chưa xác định"
            };
            _context.GiaoHangs.Add(giaoHang);

            var thongBaoNoiDung = phieuGiamGiaId.HasValue
                ? $"Bạn đã đặt đơn hàng #{donHang.Id} với tổng tiền {tongTienSauGiam:N0} VND (đã giảm {tienGiam:N0} VND). Đơn hàng đang được xử lý."
                : $"Bạn đã đặt đơn hàng #{donHang.Id} với tổng tiền {tongTienSauGiam:N0} VND. Đơn hàng đang được xử lý.";

            var thongBao = new ThongBao
            {
                TaiKhoanId = model.TaiKhoanId,
                TieuDe = "Đặt hàng thành công",
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
                message = "Đặt hàng thành công!",
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
                            message = "Không tìm thấy thông tin thanh toán"
                        });
                    }

                    // 🔹 Kiểm tra nếu đơn hàng chưa được tạo (DonHangId == null) thì tạo đơn hàng
                    if (ThanhToan.DonHangId == null && !string.IsNullOrEmpty(ThanhToan.NoiDung))
                    {
                        // Parse thông tin đơn hàng từ NoiDung (JSON)
                        var orderInfo = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(ThanhToan.NoiDung);
                        
                        var taiKhoanId = orderInfo.GetProperty("TaiKhoanId").GetInt32();
                        var tongTien = orderInfo.GetProperty("TongTien").GetDecimal();
                        var diaChiGiao = orderInfo.GetProperty("DiaChiGiao").GetString() ?? string.Empty;
                        var ghiChu = orderInfo.GetProperty("GhiChu").GetString() ?? string.Empty;
                        var phieuGiamGiaId = orderInfo.TryGetProperty("PhieuGiamGiaId", out var pgg) && pgg.ValueKind != System.Text.Json.JsonValueKind.Null 
                            ? pgg.GetInt32() 
                            : (int?)null;
                        var chiTietGioHang = orderInfo.GetProperty("ChiTietGioHang").EnumerateArray().ToList();

                        // Lấy giỏ hàng
                        var gioHang = _context.GioHangs.FirstOrDefault(g => g.TaiKhoanId == taiKhoanId);
                        if (gioHang == null)
                        {
                            return BadRequest(new { message = "Không tìm thấy giỏ hàng." });
                        }

                        // Tạo đơn hàng
                        var donHang = new DonHang
                        {
                            TaiKhoanId = taiKhoanId,
                            NgayDat = DateTime.Now,
                            TongTien = tongTien,
                            TrangThai = "Chờ xác nhận",
                            DiaChiGiao = diaChiGiao,
                            GhiChu = ghiChu,
                            PhuongThucThanhToan = true,
                            PhieuGiamGiaId = phieuGiamGiaId
                        };
                        _context.DonHangs.Add(donHang);
                        await _context.SaveChangesAsync();

                        // Lấy thông tin sản phẩm
                        var sanPhamIds = chiTietGioHang.Select(c => c.GetProperty("SanPhamId").GetInt32()).ToList();
                        var sanPhamDict = _context.SanPhams
                            .Where(sp => sanPhamIds.Contains(sp.Id))
                            .ToDictionary(sp => sp.Id, sp => sp);

                        // Tạo chi tiết đơn hàng và trừ số lượng tồn kho
                        foreach (var item in chiTietGioHang)
                        {
                            var sanPhamId = item.GetProperty("SanPhamId").GetInt32();
                            var soLuong = item.GetProperty("SoLuong").GetInt32();

                            if (sanPhamDict.TryGetValue(sanPhamId, out var sanPham))
                            {
                                // Tính giá giảm từ khuyến mãi để lưu vào đơn hàng
                                var giaGoc = sanPham.Gia;
                                var giaGiamTuKhuyenMai = TinhGiaGiamTuKhuyenMai(giaGoc, sanPham.Id);
                                
                                // Ưu tiên giá giảm từ khuyến mãi, nếu không có thì dùng giá giảm cũ hoặc giá gốc
                                var giaCuoiCung = giaGiamTuKhuyenMai < giaGoc ? giaGiamTuKhuyenMai : (sanPham.GiaGiam ?? giaGoc);
                                
                                _context.ChiTietDonHangs.Add(new ChiTietDonHang
                                {
                                    DonHangId = donHang.Id,
                                    SanPhamId = sanPhamId,
                                    SoLuong = soLuong,
                                    DonGia = giaCuoiCung // Lưu giá giảm vào đơn hàng
                                });

                                if (soLuong > sanPham.SoLuongTon)
                                {
                                    return BadRequest(new { message = "Sản phẩm không đủ số lượng trong kho." });
                                }
                                sanPham.SoLuongTon = sanPham.SoLuongTon - soLuong;
                            }
                        }

                        // Cập nhật voucher đã sử dụng (nếu có)
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

                        // Xóa giỏ hàng
                        var chiTietGioHangList = _context.ChiTietGioHangs
                            .Where(c => c.GioHangId == gioHang.Id)
                            .ToList();
                        _context.ChiTietGioHangs.RemoveRange(chiTietGioHangList);

                        // Tạo giao hàng
                        var giaoHang = new GiaoHang
                        {
                            DonHangId = donHang.Id,
                            TrangThai = "Đang chuẩn bị hàng",
                            NgayCapNhat = DateTime.Now,
                            DonViVanChuyen = "Chưa xác định"
                        };
                        _context.GiaoHangs.Add(giaoHang);

                        // Cập nhật ThanhToanTam với DonHangId
                        ThanhToan.DonHangId = donHang.Id;
                    }

                    // Cập nhật trạng thái thanh toán
                    ThanhToan.TrangThai = "đã chuyển khoản";
                    ThanhToan.IsVnPay = true;
                    _context.ThanhToanTams.Update(ThanhToan);

                    // Tạo thông báo
                    var thongbao = new ThongBao
                    {
                        TaiKhoanId = ThanhToan.TaiKhoanId,
                        TieuDe = "Đặt hàng thành công",
                        NoiDung = $"Bạn đã đặt đơn hàng #{ThanhToan.DonHangId} với tổng tiền {ThanhToan.TongTien:N0} VND với phương thức thanh toán là VNPAY. Đơn hàng đang được xử lý.",
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
                                message = "Đặt hàng thành công!",
                            });

                            Console.WriteLine($"📢 Đã gửi thông báo SignalR tới group: {state}");
                        }
                        else
                        {
                            Console.WriteLine("⚠️ State trống, không gửi được thông báo SignalR");
                        }
                    }
                    catch (Exception hubEx)
                    {
                        Console.WriteLine("❌ Lỗi khi gửi SignalR: " + hubEx.Message);
                        if (hubEx.InnerException != null)
                            Console.WriteLine("➡ Inner: " + hubEx.InnerException.Message);
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
                countdownElement.textContent = 'Tự động chuyển về trang chủ sau ' + countdown + ' giây...';
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
        <div class='checkmark'>✓</div>
        <h1>Thanh toán thành công!</h1>
        <p>Cảm ơn bạn đã mua hàng.<br>Đơn hàng của bạn đang được xử lý.</p>
        <p class='countdown' id='countdown'>Tự động chuyển về trang chủ sau 5 giây...</p>
        <a href='http://localhost:5173/' class='btn'>Về trang chủ ngay</a>
    </div>
</body>
</html>";

                    return Content(html, "text/html");

                }
                catch (Exception ex)
                {
                    return BadRequest(new { success = false, message = "Lỗi thanh toán." });
                }
            }

            return NotFound("có gì đó xảy ra rồi");
        }

    }
}

