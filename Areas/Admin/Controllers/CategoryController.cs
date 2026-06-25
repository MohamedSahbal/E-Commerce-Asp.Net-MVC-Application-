using ECommerce_Application.Services;
using ECommerce_Application.ViewModels;
using ECommerceApplication.Data;
using ECommerceApplication.Models;
using ECommerceApplication.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ECommerce_Application.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Roles.Admin)]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICategoryService _categoryService;

        public CategoryController(ApplicationDbContext context , ICategoryService categoryService)
        {
            _context = context;
            _categoryService = categoryService;
        }

        public async Task<IActionResult>Index()
        {
            var categories = await _context.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.SubCategories)
                .Include(c => c.Products)
                .OrderBy(c => c.ParentCategoryId)
                .ThenBy(c => c.Name)
                .ToListAsync();
            var parents = categories
            .Where(c => c.ParentCategoryId == null)
            .ToList();
            return View(parents);
        }

        public async Task <IActionResult> Create()
        {
            var model = new CategoryVM
            {
                ParentCategories = await _categoryService.GetParentCategoriesSelectList(null)
            };
            return View("CreateEdit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryVM model) 
        {
            if (!ModelState.IsValid)
            {
                model.ParentCategories = await _categoryService.GetParentCategoriesSelectList(null);

                return View("CreateEdit", model);
            }

            var category = new Category
            {
                Name = model.Name,
                Description = model.Description,
                ParentCategoryId = model.ParentCategoryId,
            };
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Category created.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category is null) return NotFound();
            return View("CreateEdit", new CategoryVM
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ParentCategoryId = category.ParentCategoryId,
                ParentCategories = await _categoryService.GetParentCategoriesSelectList (id)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryVM model)
        {
            if (!ModelState.IsValid)
            {
                model.ParentCategories= await _categoryService.GetParentCategoriesSelectList(model.Id);
                return View("CreateEdit",model);
            }
            var category = await _context.Categories.FindAsync(model.Id);
            if(category is null) return NotFound();
            category.Name = model.Name;
            category.Description = model.Description;
            category.ParentCategoryId = model.ParentCategoryId;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Category updated.";
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories
                .Include(c => c.SubCategories)
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (category is null) return NotFound();
            if (category.SubCategories.Any() || category.Products.Any())
            {
                TempData["Error"] = "Cannot delete category with sub-categories or products.";
                return RedirectToAction(nameof(Index));
            }
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Category deleted.";
            return RedirectToAction(nameof(Index));
        }

    }
}
