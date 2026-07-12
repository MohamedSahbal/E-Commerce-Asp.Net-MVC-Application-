using System.ComponentModel.DataAnnotations;

namespace ECommerce_Application.ViewModels
{
    public class ForgotPasswordVM
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
