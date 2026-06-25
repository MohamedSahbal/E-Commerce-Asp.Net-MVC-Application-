using ECommerce_Application.Services;
using ECommerce_Application.ViewModels;
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
    public class PromotionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICategoryService _categoryService;

        public PromotionController(ApplicationDbContext context , ICategoryService categoryService)
        {
            _context = context;
            _categoryService = categoryService;
        }
        public async Task<IActionResult> Index()
        {
            var promos = await _context.Promotions
                .Include(p => p.Category)
                .OrderByDescending(p => p.StartDate)
                .ToListAsync();
            return View(promos);
        }

        public async Task<IActionResult> Create()
        {
            return View("CreateEdit", new PromotionVM
            {
                Categories = await _categoryService.GetCategoriesSelectList()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Create(PromotionVM vm)
        {
            if (vm.EndDate <= vm.StartDate)
                ModelState.AddModelError("EndDate", "End date must be after start date.");

            if (await _context.Promotions.AnyAsync(p => p.Code == vm.Code.ToUpper()))
                ModelState.AddModelError("Code", "This code is already in use.");

            if (!ModelState.IsValid)
            {
                vm.Categories = await _categoryService.GetCategoriesSelectList();
                return View("CreateEdit",vm);
            }
            var promos = new Promotion
            {
                Name = vm.Name,
                Code = vm.Code.ToUpper(),
                DiscountPercentage = vm.DiscountPercentage,
                StartDate = vm.StartDate,
                EndDate = vm.EndDate,
                UsageLimit = vm.UsageLimit,
                IsActive = vm.IsActive,
                CategoryId = vm.CategoryId,
            };
            _context.Promotions.Add(promos);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Promotion \"{vm.Name}\" created.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var promo = await _context.Promotions.FindAsync(id);
            if (promo == null) return NotFound();
            return View("CreateEdit", new PromotionVM
            {
                Id = promo.Id,
                Name = promo.Name,
                Code = promo.Code,
                DiscountPercentage = promo.DiscountPercentage,
                StartDate = promo.StartDate,
                EndDate = promo.EndDate,
                UsageLimit = promo.UsageLimit,
                IsActive = promo.IsActive,
                CategoryId = promo.CategoryId,
                Categories = await _categoryService.GetCategoriesSelectList()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit (int id , PromotionVM vm)
        {
            if (vm.EndDate <= vm.StartDate)
                ModelState.AddModelError("EndDate", "End date must be after start date.");

            if (await _context.Promotions.AnyAsync(p => p.Code == vm.Code.ToUpper() && p.Id != vm.Id))
                ModelState.AddModelError("Code", "This code is already in use.");

            if (!ModelState.IsValid)
            {
                vm.Categories = await _categoryService.GetCategoriesSelectList();
                return View("CreateEdit", vm);
            }
            var promo = await _context.Promotions.FindAsync(id);
            if (promo == null) return NotFound();

            promo.Name = vm.Name;
            promo.Code = vm.Code.ToUpper();
            promo.DiscountPercentage = vm.DiscountPercentage;
            promo.StartDate = vm.StartDate;
            promo.EndDate = vm.EndDate;
            promo.UsageLimit = vm.UsageLimit;
            promo.IsActive = vm.IsActive;
            promo.CategoryId = vm.CategoryId;
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Promotion \"{promo.Name}\" updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task <IActionResult> Delete (int id)
        {
            var promo = await _context.Promotions.FindAsync(id);
            if (promo == null) return NotFound();
            _context.Promotions.Remove(promo);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Promotion deleted.";
            return RedirectToAction(nameof(Index));
        }


    }
}
