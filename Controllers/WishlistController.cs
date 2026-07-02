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
    public class WishlistController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public WishlistController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var items = await _context.WishlistItems
                .Include(w => w.Product).ThenInclude(p => p.Category)
                .Include(w => w.Product).ThenInclude(p => p.Images)
                .Include(w => w.Product).ThenInclude(p => p.Reviews)
                .Where(w => w.CustomerId == userId)
                .OrderByDescending(w => w.AddedAt)
                .ToListAsync();
            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int productId)
        {
            var userId = _userManager.GetUserId(User);
            var exist = await _context.WishlistItems
                .FirstOrDefaultAsync(wl => wl.CustomerId == userId);
            if (exist is not null)
            {
                _context.WishlistItems.Remove(exist);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Removed from wishlist.";
            }
            else
            {
                var productExist = await _context.Products
                    .AnyAsync(p => p.Id == productId && p.IsActive);
                if (!productExist) return NotFound();
                _context.WishlistItems.Add(new WishlistItem
                {
                    CustomerId = userId,
                    ProductId = productId
                });
                await _context.SaveChangesAsync();
                TempData["Success"] = "Product Added to wishlist!";
            }

            var referer = Request.Headers["Referer"].ToString();
            return !string.IsNullOrEmpty(referer) ? Redirect(referer) : RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int id)
        {
            var userId = _userManager.GetUserId(User);
            var item = await _context.WishlistItems
                .FirstOrDefaultAsync(wl => wl.Id == id && wl.CustomerId == userId);
            if (item == null) return NotFound();
            _context.WishlistItems.Remove(item);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Item removed from wishlist.";
            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveToCart(int id)
        {
            var userId = _userManager.GetUserId(User);
            var Item = await _context.WishlistItems
                .Include(wl => wl.Product)
                .FirstOrDefaultAsync(wl => wl.Id == id && wl.CustomerId == userId);
            if (Item == null) return NotFound();

            if (Item.Product.StockQuantity == 0)
            {
                TempData["Error"] = $"\"{Item.Product.Name}\" is out of stock.";
                return RedirectToAction(nameof(Index));
            }

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.CustomerId == userId);
            // create cart if it not found
            if (cart is null)
            {
                cart = new Cart { CustomerId = userId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }
            // if same item in cart
            var cartItem = cart.Items
                .FirstOrDefault(c => c.ProductId == Item.ProductId);
            if (cartItem is not null)
            {
                cartItem.Quantity++;
            }
            else
            {
                _context.CartItems.Add(new CartItem
                {
                    CartId = cart.Id,
                    ProductId = Item.ProductId,
                    Quantity = 1
                });
            }
            _context.WishlistItems.Remove(Item);
            cart.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"\"{Item.Product.Name}\" moved to cart.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Clear()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Forbid();
            var items = await _context.WishlistItems
                .Where(w => w.CustomerId == userId)
                .ToListAsync();
            _context.WishlistItems.RemoveRange(items);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Wishlist cleared.";
            return RedirectToAction(nameof(Index));

        }
    }

}
