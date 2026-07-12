

namespace ECommerce_Application.Services.Email_Service
{
    public interface IEmailService
    {
        Task SendOtpAsync(string email, string otp);
    }
}
