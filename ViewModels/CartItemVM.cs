namespace ECommerce_Application.ViewModels
{
    public class CartItemVM
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public int MaxQuantity { get; set; }
        public decimal LineTotal => UnitPrice * Quantity;
    }
}
