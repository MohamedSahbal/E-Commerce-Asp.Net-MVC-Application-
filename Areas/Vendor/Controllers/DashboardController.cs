using ECommerceApplication.Utilities;
using ECommerceApplication.Data;
using ECommerceApplication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce_Application.Areas.Vendor.Controllers
{

    [Area("Vendor")]
    [Authorize(Roles = Roles.Vendor)]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.VendorStatus != VendorStatus.Approved)
            {
                return RedirectToAction("PendingApproval", "Account", new { area = "" });

            }
            var vendorId = user.Id;

            ViewBag.TotalProducts = await _context.Products.CountAsync(p => p.VendorId == vendorId);
            ViewBag.TotalOrders = await _context.OrderItems
                .Where(o => o.VendorId == vendorId)
                .Select(o => o.OrderId)
                .Distinct()
                .CountAsync();
            ViewBag.TotalRevenue = await _context.OrderItems
                .Where(o => o.VendorId == vendorId && o.Order.Status != OrderStatus.Cancelled)
                .SumAsync(o => (decimal?)(o.UnitPrice * o.Quantity)) ?? 0;
            ViewBag.LowStock = await _context.Products
          .CountAsync(p => p.VendorId == vendorId && p.StockQuantity < 5);

            return View();
        }

    }
    
}
