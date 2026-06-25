using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceApplication.Models;

public class OrderItem
{
    public int Id { get; set; }
    public int Quantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public string VendorId { get; set; } = string.Empty;
    public ApplicationUser Vendor { get; set; } = null!;
}
