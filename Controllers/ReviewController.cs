using ECommerceApplication.Data;
using ECommerceApplication.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce_Application.Controllers
{
    public class ReviewController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReviewController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> create(int productId, int rating, string? comment)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Forbid();
            bool alreadyReviewed = _context.Reviews
                .Any(r => r.ProductId == productId && r.CustomerId == user.Id);
            if (alreadyReviewed)
            {
                TempData["Error"] = "You have already reviewed this product.";
                return RedirectToAction("Details", "Shop", new { id = productId });
            }
            if (rating < 1 || rating > 5)
            {
                TempData["Error"] = "Please select a rating between 1 and 5.";
                return RedirectToAction("Details", "Shop", new { id = productId });
            }
            _context.Reviews.Add(new Review
            {
                ProductId = productId,
                CustomerId = user.Id,
                Rating = rating,
                Comment = comment?.Trim()
            });
            await _context.SaveChangesAsync();

            TempData["Success"] = "Your review has been submitted.";
            return RedirectToAction("Details", "Shop", new { id = productId, fragment = "reviews" });
        }
    }
}
