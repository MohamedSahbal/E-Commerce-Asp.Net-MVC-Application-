using ECommerceApplication.Data;
using ECommerceApplication.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace ECommerce_Application.Controllers
{
    public class ShopController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const int PageSize = 12;

        public ShopController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task <IActionResult> Index(
            string? search,
            int? categoryId,
            decimal? minPrice,
            decimal? maxPrice,
            string sortedBy = "newest",
            int page = 1)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Vendor)
                .Include(p => p.Images)
                .Include(p => p.Reviews)
                .Where(p => p.IsActive && p.StockQuantity > 0)
                .AsQueryable();

            //searching
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.Name.Contains(search) 
                || (p.Description != null && p.Description.Contains(search)));

            // get the category and its subs
            if (categoryId.HasValue)
            {
                var subs= await _context.Categories
                    .Where(c=>c.Id == categoryId || c.ParentCategoryId == categoryId)
                    .Select(c=>c.Id)
                    .ToListAsync();
                // get only subs
                query = query.Where(p =>
                 subs.Contains(p.CategoryId));
            }
            // search by price
            if (minPrice.HasValue) query = query.Where(p => p.Price >= minPrice);
            if (maxPrice.HasValue) query = query.Where(p => p.Price <= maxPrice);
            query = sortedBy switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "name" => query.OrderBy(p => p.Name),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };
            // pagination
            int total= await query.CountAsync();
            int totalPages = (int)Math.Ceiling(total / (double)PageSize);
            page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

            var products = await query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
            string? activeCategoryName = null;
            if (categoryId.HasValue)
                activeCategoryName = (await _context.Categories.FindAsync(categoryId))?.Name;
            var vm = new ShopVM
            {
                Products = products,
                Categories = await _context.Categories
               .Include(c => c.SubCategories)
               .Where(c => c.ParentCategoryId == null)
               .OrderBy(c => c.Name)
               .ToListAsync(),
                Search = search,
                CategoryId = categoryId,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                SortBy = sortedBy,
                Page = page,
                TotalPages = totalPages,
                TotalCount = total,
                PageSize = PageSize,
                ActiveCategoryName = activeCategoryName
            };

            return View(vm);
        }
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .ThenInclude(c => c.ParentCategory)
                .Include(p => p.Vendor)
                .Include(p => p.Images)
                .Include(p => p.Reviews).ThenInclude(r => r.Customer)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            if (product is null) return NotFound();

            // Related products: same category, exclude current
            var related = await _context.Products
                .Include(p => p.Images)
                .Include(p => p.Reviews)
                .Where(p => p.CategoryId == product.CategoryId && p.Id != id && p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .Take(4)
                .ToListAsync();

            ViewBag.RelatedProducts = related;

            // Check if current user already reviewed / wishlisted this product
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = await _context.Users
                    .Where(u => u.UserName == User.Identity.Name)
                    .Select(u => u.Id)
                    .FirstOrDefaultAsync();

                ViewBag.UserReview = product.Reviews.FirstOrDefault(r => r.CustomerId == userId);
                ViewBag.UserId = userId;
                ViewBag.IsWishlisted = await _context.WishlistItems
                    .AnyAsync(w => w.CustomerId == userId && w.ProductId == id);
            }

            return View(product);
        }

    }
}
