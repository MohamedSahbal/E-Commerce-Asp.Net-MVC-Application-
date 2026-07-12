using System.ComponentModel.DataAnnotations;

namespace ECommerce_Application.ViewModels
{
    public class ResetPasswordVM
    {
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
