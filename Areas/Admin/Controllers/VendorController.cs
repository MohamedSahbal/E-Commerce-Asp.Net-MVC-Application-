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
    public class VendorController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public VendorController(UserManager<ApplicationUser> userManager , ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var Vendors= await _context.Users
                .Where(u => u.VendorStatus != VendorStatus.NotApplicable)
                .OrderBy(u => u.VendorStatus)
                .ThenBy(u => u.CreatedAt)
                .ToListAsync();
            return View(Vendors);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Approve(String id) 
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)  return NotFound();

            user.VendorStatus = VendorStatus.Approved;
            await _userManager.UpdateAsync(user);
            TempData["Success"] = $"{user.FirstName} {user.LastName} approved as a vendor.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectAsync(String id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.VendorStatus = VendorStatus.Rejected;
            await _userManager.UpdateAsync(user);
            TempData["Success"] = $"{user.FirstName} {user.LastName}'s vendor application rejected.";
            return RedirectToAction(nameof(Index));
        }




    }
}

