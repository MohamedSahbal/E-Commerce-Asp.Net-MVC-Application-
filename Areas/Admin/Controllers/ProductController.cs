using ECommerce_Application.Services;
using ECommerceApp.Services;
using ECommerceApplication.Data;
using ECommerceApplication.Models;
using ECommerceApplication.Utilities;
using ECommerceApplication.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ECommerce_Application.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Roles.Admin)]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CloudinaryService _cloudinary;
        private readonly IProductImageService _productImageService;
        private readonly ICategoryService _categoryService;

        public ProductController(ApplicationDbContext context, CloudinaryService cloudinary,IProductImageService productImageService
            , ICategoryService categoryService )
        {
            _context = context;
            _cloudinary = cloudinary;
            _productImageService = productImageService;
            _categoryService = categoryService;
        }
        public async Task<IActionResult> Index(string? search , int? categoryId)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p =>p.Vendor)
                .Include(p => p.Images)
                .AsQueryable();
            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => p.Name.Contains(search));
            if(categoryId.HasValue)
                query=query.Where(p=>p.CategoryId == categoryId);
            
            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;
            ViewBag.Categories = new SelectList(await _context.Categories.OrderBy(c => c.Name).ToListAsync(), "Id", "Name");

            return View(await query.OrderByDescending(p => p.CreatedAt).ToListAsync());
        }
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Vendor)
                .Include(p => p.Images)
                .Include(p => p.Reviews)
                .ThenInclude(r => r.Customer)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product is null) return NotFound();
            return View(product);
        }
        public async Task<IActionResult> Create()
        {
            return View("Create", new ProductVM
            {
                Categories = await _categoryService.GetCategoriesSelectList()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductVM model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = await _categoryService.GetCategoriesSelectList();
                return View("Create", model);

            }
            var adminId = (await _context.Users.FirstAsync(u => u.Email == "admin@ecommerce.com")).Id;

            var product = new Product
            {
                Name = model.Name,
                Description = model.Description,
                Price = model.Price,
                StockQuantity = model.StockQuantity,
                CategoryId = model.CategoryId,
                IsActive = model.IsActive,
                VendorId = adminId
            };
            _context.Products.Add(product);
            _context.SaveChanges();
            await _productImageService.UploadImages(model.NewImages, product);
            TempData["Success"] = $"Product \"{product.Name}\" created.";
            return RedirectToAction(nameof(Index));

        }

        public async Task<IActionResult> Edit(int id)
        { 
        var product = await _context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);
        if (product is null) return NotFound();
            return View("Edit", new ProductVM
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                CategoryId = product.CategoryId,
                IsActive = product.IsActive,
                Categories = await _categoryService.GetCategoriesSelectList(),
                ExistingImages = await _productImageService.GetExistingImages(product.Id)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductVM model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = await _categoryService.GetCategoriesSelectList();
                model.ExistingImages = await _productImageService.GetExistingImages(model.Id);
                return View("Edit", model);

            }
            var product = await _context.Products.Include(p => p.Images)
               .FirstOrDefaultAsync(p => p.Id == model.Id);
            if (product is null) return NotFound();

            product.Name = model.Name;
            product.Description = model.Description;
            product.Price = model.Price;
            product.StockQuantity = model.StockQuantity;
            product.CategoryId = model.CategoryId;
            product.IsActive = model.IsActive;
         
                await _context.SaveChangesAsync();
            if (model.NewImages?.Any(f => f != null && f.Length > 0) == true)
            {
                await _productImageService.UploadImages(
                model.NewImages,
                product);
            }
            TempData["Success"] = $"Product \"{product.Name}\" updated Successfully.";
            return RedirectToAction(nameof(Index));

        }
        public async Task<IActionResult> DeleteImage(int id, int productId)
        {
           await _productImageService.DeleteImage(id);
            return RedirectToAction(nameof(Edit), new { id = productId });
        }
        public async Task<IActionResult> SetMainImage(int id, int productId)
        {
            await _productImageService.SetMainImage(id, productId);
            return RedirectToAction(nameof(Edit), new { id = productId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            foreach (var image in product.Images.ToList())
            {
                await _productImageService.DeleteImage(image.Id);
            }

            _context.Products.Remove(product);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Product deleted Successfully.";

            return RedirectToAction(nameof(Index));
        }




    }
}
