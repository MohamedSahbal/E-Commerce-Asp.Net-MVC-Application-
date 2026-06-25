using ECommerceApplication.Data;
using ECommerceApplication.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ECommerce_Application.Services.Cart_Service
{
    public class CartService : ICartService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public CartService(UserManager<ApplicationUser> userManager , ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }
        public async Task<Cart?> GetCartWithDetailsAsync(string? userId)
        {
            return await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .ThenInclude(p => p.Category)
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .ThenInclude(p => p.Images)
                .Include(c => c.AppliedPromotion)
                .ThenInclude(p => p!.Category)
                .FirstOrDefaultAsync(c => c.CustomerId == userId);
        }

    }
   
}
