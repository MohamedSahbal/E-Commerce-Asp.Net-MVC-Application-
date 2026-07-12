namespace ECommerce_Application.Models
{
    public class PasswordResetOTP
    {
        public int Id { get; set; }

        public string Email { get; set; }

        public string OTP { get; set; }

        public DateTime ExpireAt { get; set; }

        public bool IsUsed { get; set; }
    }
}
