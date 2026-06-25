using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ECommerceApplication.ViewModels
{
    public class ProductVM
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required, Range(0.01, 999999)]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Required, Range(0, 99999)]
        public int StockQuantity { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public bool IsActive { get; set; } = true;

        public List<IFormFile> NewImages { get; set; } = new();

        // Populated on Edit — existing images already on Cloudinary
        public List<ProductImageVM> ExistingImages { get; set; } = new();

        // For dropdowns
        public SelectList? Categories { get; set; }
    }

  

}

