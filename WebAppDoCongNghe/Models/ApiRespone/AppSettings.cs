namespace WebAppDoCongNghe.Models.ApiRespone
{
    public class AppSettings
    {
        public string Key { get; set; } 
        public string Issuer { get; set; } 
        public string Audience { get; set; } 
        public int ExpireHours { get; set; }
    }
}
