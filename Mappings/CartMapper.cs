using ECommerce_Application.ViewModels;
using ECommerceApplication.Models;

namespace ECommerce_Application.Mappings
{
    public class CartMapper
    {
        public static CartVM BuildViewModel(Cart? cart) => new()
        {
            AppliedPromotion = cart.AppliedPromotion,
            Items = cart.Items.Select(i =>
            {
                var mainImg = i.Product.Images.FirstOrDefault(img => img.IsMain)
                           ?? i.Product.Images.FirstOrDefault();
                return new CartItemVM
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product.Name,
                    ImageUrl = mainImg?.ImageUrl,
                    CategoryName = i.Product.Category.Name,
                    CategoryId = i.Product.CategoryId,
                    UnitPrice = i.Product.Price,
                    Quantity = i.Quantity,
                    MaxQuantity = i.Product.StockQuantity
                };
            }).ToList()
        };
    }


}

