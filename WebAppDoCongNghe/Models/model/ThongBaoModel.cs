namespace WebAppDoCongNghe.Models.model
{
    public class ThongBaoModel
    {
        public int TaiKhoanId { get; set; }   // ID người nhận thông báo
        public string? TieuDe { get; set; }   // Tiêu đề thông báo
        public string? NoiDung { get; set; }  // Nội dung chi tiết
    }
}
