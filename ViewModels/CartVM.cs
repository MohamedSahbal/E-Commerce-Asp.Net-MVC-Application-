using ECommerceApplication.Models;

namespace ECommerce_Application.ViewModels
{
    public class CartVM
    {
        public List<CartItemVM> Items { get; set; } = new();
        public Promotion? AppliedPromotion { get; set; }
        public string? PromoError { get; set; }

        public decimal SubTotal => Items.Sum(i => i.LineTotal);

        public decimal DiscountAmount
        {
            get
            {
                if (AppliedPromotion is null) return 0;

                if (AppliedPromotion.CategoryId.HasValue)
                {
                    var applicable = Items
                        .Where(i => i.CategoryId == AppliedPromotion.CategoryId)
                        .Sum(i => i.LineTotal);
                    return Math.Round(applicable * AppliedPromotion.DiscountPercentage / 100, 2);
                }

                // Site-wide
                return Math.Round(SubTotal * AppliedPromotion.DiscountPercentage / 100, 2);
            }
        }

        public decimal Total => Math.Max(0, SubTotal - DiscountAmount);
    }
}
