using System.ComponentModel.DataAnnotations;

namespace ECommerceApplication.ViewModels
{
    public class ProfileVM
    {
        [Required, MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Address { get; set; }

        [Phone]
        public string? Phone { get; set; }

        public string? Email { get; set; }
    }
}
