namespace ECommerceApplication.Models;

public class WishlistItem
{
    public int Id { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    public string CustomerId { get; set; } = string.Empty;
    public ApplicationUser Customer { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
}
