namespace HMS.Models
{
    public class LoginSession : BaseEntity
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string? OtpCode { get; set; }
        public string? DeviceInfo { get; set; }
        public string? IpAddress { get; set; }
        public bool IsLoggedIn { get; set; }
    }
}
