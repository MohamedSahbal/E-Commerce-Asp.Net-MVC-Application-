using System.ComponentModel.DataAnnotations;

namespace ECommerce_Application.ViewModels
{
    public class CheckoutVM
    {
        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string AddressLine1 { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? AddressLine2 { get; set; }

        [Required, MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string StateRegion { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string PostalCode { get; set; } = string.Empty;

        [Required, MaxLength(60)]
        public string Country { get; set; } = "Egypt";

        [Phone, MaxLength(30)]
        public string? Phone { get; set; }

        // Cart summary (read-only, populated by controller)
        public CartVM? Cart { get; set; }

        public string FormattedAddress =>
            $"{FullName}, {AddressLine1}" +
            (string.IsNullOrEmpty(AddressLine2) ? "" : $", {AddressLine2}") +
            $", {City}, {StateRegion} {PostalCode}, {Country}" +
            (string.IsNullOrEmpty(Phone) ? "" : $" | Tel: {Phone}");
    }

}

