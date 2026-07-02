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

    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrdersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Forbid();
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.CustomerId == userId)
                .OrderBy(o => o.CreatedAt)
                .ToListAsync();
            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Forbid();

            var order = await _context.Orders
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product).ThenInclude(p => p.Images)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Vendor)
                .Include(o => o.Promotion)
                .FirstOrDefaultAsync(o => o.Id == id && o.CustomerId == userId);
            if (order is null) return NotFound();
            return View(order);
        }
    }
    
}
