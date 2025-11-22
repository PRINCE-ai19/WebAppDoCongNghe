namespace WebAppDoCongNghe.Models.model
{
    public class DatHangRequest
    {
        public int payment { get; set; }
        public int TaiKhoanId { get; set; }
        public string? DiaChiGiao { get; set; }
        public string? GhiChu { get; set; }
        public string? PhuongThucThanhToan { get; set; }
        public string? MaPhieuGiamGia { get; set; } // Mã phiếu giảm giá (optional)
    }
}
