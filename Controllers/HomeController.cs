using System.Diagnostics;
using ECommerceApplication.Data;
using ECommerceApplication.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce_Application.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<HomeController> _logger;
        public HomeController(ApplicationDbContext db, ILogger<HomeController> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var featured = await _db.Products
                .Where(p => p.IsActive && p.StockQuantity > 0)
                .Include(p => p.Images)
                .Include(p => p.Reviews)
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedAt)
                .Take(8)
                .ToListAsync();

            var categories = await _db.Categories
                .Where(c => c.ParentCategoryId == null)
                .Include(c => c.SubCategories)
                .OrderBy(c => c.Name)
                .Take(6)
                .ToListAsync();

            ViewBag.FeaturedProducts = featured;
            ViewBag.TopCategories = categories;
            ViewBag.TotalProducts = await _db.Products.CountAsync(p => p.IsActive);
            ViewBag.TotalVendors = await _db.Users.CountAsync(u => u.VendorStatus == VendorStatus.Approved);

            return View();
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
