using Microsoft.AspNetCore.Identity;

namespace ECommerceApplication.Models;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public VendorStatus VendorStatus { get; set; } = VendorStatus.NotApplicable;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
    public Cart? Cart { get; set; }
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<OrderItem> OrderItems { get; internal set; } = new List<OrderItem>();
}

public enum VendorStatus
{
    NotApplicable,
    Pending,
    Approved,
    Rejected
}
