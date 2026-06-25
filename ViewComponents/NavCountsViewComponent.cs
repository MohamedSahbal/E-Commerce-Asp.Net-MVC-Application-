using ECommerceApplication.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ECommerceApplication.Models;

namespace ECommerceApplication.ViewComponents;

public class NavCountsViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public NavCountsViewComponent(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        int cartCount = 0;
        int wishlistCount = 0;

        if (UserClaimsPrincipal.Identity?.IsAuthenticated == true)
        {
            var userId = _userManager.GetUserId(UserClaimsPrincipal);
            if (userId is not null)
            {
                cartCount = await _db.CartItems
                    .Where(ci => ci.Cart.CustomerId == userId)
                    .SumAsync(ci => (int?)ci.Quantity) ?? 0;

                wishlistCount = await _db.WishlistItems
                    .CountAsync(w => w.CustomerId == userId);
            }
        }

        return View(new NavCountsModel { CartCount = cartCount, WishlistCount = wishlistCount });
    }
}

public record NavCountsModel(int CartCount = 0, int WishlistCount = 0);
