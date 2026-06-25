using ECommerce_Application.Mappings;
using ECommerce_Application.Services.Cart_Service;
using ECommerce_Application.ViewModels;
using ECommerceApplication.Data;
using ECommerceApplication.Models;
using ECommerceApplication.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce_Application.Controllers
{
    [Authorize(Roles = Roles.Customer)]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICartService _cartService;
        public CartController(ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager
            ,ICartService cartService)
        {
            _context = context;
            _userManager = userManager;
            _cartService = cartService;
        }
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var cart= await _cartService.GetCartWithDetailsAsync(userId);
            if (cart == null) return View(new CartVM());
            var model = CartMapper.BuildViewModel(cart);
                return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add (int productId , int quantity = 1)
        {
            var product = await _context.Products.FindAsync(productId);
            if(product == null || !product.IsActive) return NotFound();

            var userId = _userManager.GetUserId(User);
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.CustomerId == userId);
            if(cart is null) 
            {
            cart = new Cart { CustomerId = userId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }
            // product already exist
            var exist = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (exist is not null)
            {
                exist.Quantity =
                    Math.Min(exist.Quantity + quantity,
                             product.StockQuantity);
            }
            else
            {
                _context.CartItems.Add(new CartItem
                {
                    CartId = cart.Id,
                    ProductId = productId,
                    Quantity = Math.Min(quantity, product.StockQuantity)
                });
            }
            cart.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            TempData["Success"] = $"\"{product.Name}\" added to cart.";
            // return to the page that I use it
            var referer = Request.Headers["Referer"].ToString();
            return !string.IsNullOrEmpty(referer) ? Redirect(referer) : RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int itemId , int quantity)
        {
            var userId = _userManager.GetUserId(User)!;
            var item = await _context.CartItems
                .Include(ci => ci.Cart)
                .Include(ci => ci.Product)
                .FirstOrDefaultAsync(ci => ci.Id == itemId && ci.Cart.CustomerId == userId);

            if (item is null) return NotFound();

            if (quantity <= 0)
            {
                _context.CartItems.Remove(item);
            }
            else
            {
                item.Quantity = Math.Min(quantity, item.Product.StockQuantity);
                item.Cart.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int itemId)
        {
            var userId = _userManager.GetUserId(User)!;
            var item = await _context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.Id == itemId && ci.Cart.CustomerId == userId);
            if(item is not null)
            {
                _context.CartItems.Remove(item);
                item.Cart.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task <IActionResult> Clear()
        {
            var userId = _userManager.GetUserId(User);
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c =>c.CustomerId == userId);
            if (cart is not null)
            {
                _context.CartItems.RemoveRange(cart.Items);
                cart.AppliedPromotionId = null;
                cart.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Cart cleared.";
            return RedirectToAction(nameof(Index));

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task <IActionResult> ApplyPromotion(string code)
        {
            var userId = _userManager.GetUserId(User);
            var cart = await _cartService.GetCartWithDetailsAsync(userId);
            if (cart is null || !cart.Items.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction(nameof(Index));
            }
            var now = DateTime.UtcNow;
            var promo = await _context.Promotions
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Code.ToUpper() == code.ToUpper().Trim()
                                   && p.IsActive
                                   && p.StartDate <= now
                                   && p.EndDate >= now);

            if (promo is null)
            {
                TempData["Error"] = "Invalid or expired promo code.";
                return RedirectToAction(nameof(Index));
            }

            if (promo.UsageLimit.HasValue && promo.UsageCount >= promo.UsageLimit)
            {
                TempData["Error"] = "This promo code has reached its usage limit.";
                return RedirectToAction(nameof(Index));
            }

            // For category-specific promotions, check cart has at least one matching item
            if (promo.CategoryId.HasValue)
            {
                var hasmatchedItems = cart.Items.Any(i => i.Product.CategoryId == promo.CategoryId);
                if (!hasmatchedItems)
                {
                    TempData["Error"] = $"This promo only applies to {promo.Category?.Name} products.";
                    return RedirectToAction(nameof(Index));
                }
            }
                cart.AppliedPromotionId = promo.Id;
                cart.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Promo \"{promo.Name}\" applied — {promo.DiscountPercentage}% off!";
                return RedirectToAction(nameof(Index));
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RemovePromotion()
        {
            var userId = _userManager.GetUserId(User)!;
            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.CustomerId == userId);
            if (cart is not null)
            {
                cart.AppliedPromotionId = null;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

    }
}


