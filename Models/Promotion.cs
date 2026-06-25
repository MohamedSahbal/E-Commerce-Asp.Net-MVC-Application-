using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceApplication.Models;

public class Promotion
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    public string Code { get; set; } = string.Empty;

    [Column(TypeName = "decimal(5,2)")]
    [Range(1, 100)]
    public decimal DiscountPercentage { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public int? UsageLimit { get; set; }
    public int UsageCount { get; set; }

    public bool IsActive { get; set; } = true;

    // null = site-wide, set = category-specific
    public int? CategoryId { get; set; }
    public Category? Category { get; set; }

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
