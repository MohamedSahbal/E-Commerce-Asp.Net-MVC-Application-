using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ECommerce_Application.ViewModels
{
    public class PromotionVM
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(30)]
        [RegularExpression(@"^[A-Z0-9]+$", ErrorMessage = "Code must be uppercase letters and numbers only.")]
        public string Code { get; set; } = string.Empty;

        [Required, Range(1, 100)]
        public decimal DiscountPercentage { get; set; }

        [Required, DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Required, DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; } = DateTime.Today.AddMonths(1);

        public int? UsageLimit { get; set; }

        public bool IsActive { get; set; } = true;

        public int? CategoryId { get; set; }
        public SelectList? Categories { get; set; }
    }

}

