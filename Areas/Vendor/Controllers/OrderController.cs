using ECommerceApplication.Data;
using ECommerceApplication.Models;
using ECommerceApplication.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce_Application.Areas.Vendor.Controllers
{
    [Area("Vendor")]
    [Authorize(Roles = Roles.Vendor)]
    public class OrderController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public OrderController(UserManager<ApplicationUser> userManager , ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }
            var VendorId=user.Id;
            // get order Items
            var orderIds = await _context.OrderItems
             .Where(oi => oi.VendorId == VendorId)
             .Select(oi => oi.OrderId)
             .Distinct()
             .ToListAsync();

            var Orders = await _context.Orders
                .Include(o=>o.Customer)
                .Include(o=>o.OrderItems.Where(oi=> oi.VendorId == VendorId))
                .ThenInclude(oi=>oi.Product)
                .Where(o => orderIds.Contains(o.Id))
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return View(Orders);
        }

        public async Task <IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }
            var VendorId = user.Id;

            var Order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems.Where(oi => oi.VendorId == VendorId))
                    .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.Images)
                .FirstOrDefaultAsync(o => o.Id == id &&
                    o.OrderItems.Any(oi => oi.VendorId == VendorId));
            if (Order == null)
            {
                return NotFound();
            }
            return View(Order);

        }

    }
}
