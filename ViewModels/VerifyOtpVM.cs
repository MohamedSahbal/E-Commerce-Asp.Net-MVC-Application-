using System.ComponentModel.DataAnnotations;

namespace ECommerce_Application.ViewModels
{
    public class VerifyOtpVM
    {
        public string Email { get; set; }

        [Required]
        public string OTP { get; set; }
    }
}
