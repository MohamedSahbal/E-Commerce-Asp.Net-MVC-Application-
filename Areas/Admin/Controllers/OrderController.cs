using ECommerceApplication.Data;
using ECommerceApplication.Models;
using ECommerceApplication.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce_Application.Areas.Admin.Controllers
{

    [Area("Admin")]
    [Authorize(Roles = Roles.Admin)]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? status)
        {
            var query = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, out var s))
                query = query.Where(o => o.Status == s);

            ViewBag.StatusFilter = status;
            var filteredOrders = await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
            return View(filteredOrders);
        }
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include (o => o.Customer)
                .Include (o => o.OrderItems).ThenInclude(oi => oi.Product)
                .ThenInclude(p => p.Images)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Vendor)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order is null) return NotFound();
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus status) 
        {
            var order = await _context.Orders.FindAsync(id);
            if (order is null) return NotFound();
            order.Status = status;
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Order #{id} status updated to {status}.";
            return RedirectToAction(nameof(Details), new { id });

        }


    }

}
