using ECommerceApplication.Data;
using ECommerceApplication.Models;
using ECommerceApplication.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce_Application.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Roles.Admin)]
    public class DashboardController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public DashboardController(UserManager<ApplicationUser> userManager , ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalProducts = await _context.Products.CountAsync();
            ViewBag.TotalOrders = await _context.Orders.CountAsync();
            ViewBag.TotalCustomers = await _context.Users
                .Where(u => u.VendorStatus == VendorStatus.NotApplicable).CountAsync();
            ViewBag.PendingVendors = await _context.Users
                .Where(u => u.VendorStatus == VendorStatus.Pending).CountAsync();
            ViewBag.TotalRevenue = await _context.Orders
                .Where(o => o.Status != OrderStatus.Cancelled)
               .SumAsync(o => (decimal?)o.Total) ?? 0;
            return View();
        }

    }
    }

