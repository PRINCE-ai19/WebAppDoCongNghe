namespace WebAppDoCongNghe.Models.ApiRespone
{
    public static class OrderStatus
    {
        public const string ChoXuLy = "Chờ xử lý";
        public const string DangChuanBi = "Đang chuẩn bị hàng";
        public const string DangGiaoHang = "Đang giao hàng";
        public const string GiaoThanhCong = "Giao thành công";
        public const string DaHuy = "Đã hủy";

        private static readonly Dictionary<string, string[]> _transitions = new()
        {
            { ChoXuLy, new[] { DangChuanBi, DaHuy } },
            { DangChuanBi, new[] { DangGiaoHang, DaHuy } },
            { DangGiaoHang, new[] { GiaoThanhCong } },
            { GiaoThanhCong, Array.Empty<string>() },
            { DaHuy, Array.Empty<string>() }
        };

        public static bool CanTransit(string? currentStatus, string targetStatus)
        {
            currentStatus ??= string.Empty;
            if (!_transitions.TryGetValue(currentStatus, out var targets))
            {
                return false;
            }
            return targets.Contains(targetStatus);
        }

        public static bool CanCancel(string? currentStatus)
        {
            return currentStatus == ChoXuLy || currentStatus == DangChuanBi;
        }
    }
}

