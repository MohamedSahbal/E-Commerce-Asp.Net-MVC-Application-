namespace ECommerceApplication.ViewModels
{
    public class ProductImageVM
    {
        
            public int Id { get; set; }
            public string ImageUrl { get; set; } = string.Empty;
            public string PublicId { get; set; } = string.Empty;
            public bool IsMain { get; set; }
        
    }
}
