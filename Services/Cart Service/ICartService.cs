using ECommerce_Application.ViewModels;
using ECommerceApplication.Models;

namespace ECommerce_Application.Services.Cart_Service
{
    public interface ICartService
    {
        Task<Cart?> GetCartWithDetailsAsync(string? userId);
    }
}
