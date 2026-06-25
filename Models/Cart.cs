namespace ECommerceApplication.Models;

public class Cart
{
    public int Id { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string CustomerId { get; set; } = string.Empty;
    public ApplicationUser Customer { get; set; } = null!;

    public int? AppliedPromotionId { get; set; }
    public Promotion? AppliedPromotion { get; set; }

    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}
