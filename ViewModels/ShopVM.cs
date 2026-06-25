using ECommerceApplication.Models;

namespace ECommerceApplication.ViewModels
{
    public class ShopVM
    {
        public List<Product> Products { get; set; } = new();
        public List<Category> Categories { get; set; } = new();

        // Active filters
        public string? Search { get; set; }
        public int? CategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string SortBy { get; set; } = "newest";

        // Pagination
        public int Page { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public int PageSize { get; set; } = 12;

        public bool HasPrev => Page > 1;
        public bool HasNext => Page < TotalPages;

        public string? ActiveCategoryName { get; set; }
    }
}
