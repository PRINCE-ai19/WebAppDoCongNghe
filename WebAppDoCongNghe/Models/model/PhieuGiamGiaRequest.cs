namespace WebAppDoCongNghe.Models.model
{
    public class PhieuGiamGiaRequest
    {
        public string MaPhieu { get; set; }
        public string MoTa { get; set; }
        public decimal GiaTriGiam { get; set; }
        public string KieuGiam { get; set; }
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }
        public int SoLuong { get; set; }
     
        public bool TrangThai { get; set; }
    }
}
