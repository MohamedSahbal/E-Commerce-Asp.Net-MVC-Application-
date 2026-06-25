namespace ECommerceApplication.Models;

public class ProductImage
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string PublicId { get; set; } = string.Empty;
    public bool IsMain { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
}
