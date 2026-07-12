using ECommerceApplication.Data;
using ECommerceApplication.Utilities;
using ECommerceApplication.ViewModels;
using ECommerceApp.Services;
using ECommerceApplication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ECommerce_Application.Services;

namespace ECommerce_Application.Areas.Vendor.Controllers
{

    [Area("Vendor")]
    [Authorize(Roles = Roles.Vendor)]
    public class ProductController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly CloudinaryService _cloudinary;
        private readonly IProductImageService _productImageService;

        public ProductController(UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            CloudinaryService cloudinary,
            IProductImageService productImageService)
        {
            _userManager = userManager;
            _context = context;
            _cloudinary = cloudinary;
            _productImageService = productImageService;
        }
        public async Task <string?> GetVendorIdAsync()
        {
            var user= await _userManager.GetUserAsync(User);
            if(user?.VendorStatus == VendorStatus.Approved)
            {
                return user.Id;
            }
            return null;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var vendorId= await GetVendorIdAsync();
            if(vendorId == null) {
            return RedirectToAction("PendingApproval", "Account");
            }
            // product for the specific vendor
            var products = await _context.Products
                .Include(p=>p.Category)
                .Include(p=>p.Images)
                .Where(p=>p.VendorId == vendorId)
                .OrderByDescending(p=>p.CreatedAt) // newest created
                .ToListAsync();
            return View(products);
        }

        [HttpGet]
        public async Task <IActionResult> Create()
        {
            return View("Create", new ProductVM
            {
                IsActive = true,
                Categories = await GetCategoriesSelectList()
            });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductVM model)
        {
            var vendorId = await GetVendorIdAsync();
            if (vendorId == null)
            {
                return Forbid();
            }
            if (!ModelState.IsValid)
            {
                model.Categories = await GetCategoriesSelectList();
                return View("Create", model);
            }
            var product = new Product()
            {
                Name = model.Name,
                Description = model.Description,
                Price = model.Price,
                StockQuantity = model.StockQuantity,
                CategoryId = model.CategoryId,
                IsActive = model.IsActive,
                VendorId = vendorId,
            };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            await _productImageService.UploadImages(
                model.NewImages,
                product); 
            TempData["Success"] = $"Product \"{product.Name}\" created.";
            return RedirectToAction(nameof(Index));
        }

        public async Task <IActionResult> Edit(int id)
        {
            var vendorId = await GetVendorIdAsync();
            if (vendorId == null)
            {
                return Forbid();
            }
            var product = await _context.Products.Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id && p.VendorId == vendorId);
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
                Categories = await GetCategoriesSelectList(),
                ExistingImages = await _productImageService.GetExistingImages(product.Id)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task <IActionResult> Edit(ProductVM model)
        {
            var vendorId = await GetVendorIdAsync();
            if (vendorId == null)
            {
                return Forbid();
            }
            if (!ModelState.IsValid)
            {
                model.Categories= await GetCategoriesSelectList();
                model.ExistingImages = await _productImageService.GetExistingImages(model.Id);
                return View("Edit", model);

            }
            var product = await _context.Products.Include(p => p.Images)
               .FirstOrDefaultAsync(p => p.Id == model.Id && p.VendorId == vendorId);
            if (product is null) return NotFound();

            product.Name = model.Name;
            product.Description = model.Description;
            product.Price = model.Price;
            product.StockQuantity = model.StockQuantity;
            product.CategoryId = model.CategoryId;
            product.IsActive = model.IsActive;

            await _context.SaveChangesAsync();
            await _productImageService.UploadImages(
                model.NewImages,
                product);

            TempData["Success"] = $"Product \"{product.Name}\" updated Successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var vendorId = await GetVendorIdAsync();
            if (vendorId == null)
            {
                return Forbid();
            }
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
       

       
                [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImage(int id,int productId)
        {
            await _productImageService.DeleteImage(id);

            return RedirectToAction(nameof(Edit), new { id = productId });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetMainImage(int id, int productId)
        {
            await _productImageService.SetMainImage(
                id,
                productId);

            return RedirectToAction(nameof(Edit),
                new { id = productId });
        }

       
      

        // get all categories
        private async Task<SelectList> GetCategoriesSelectList() =>
       new SelectList(await _context.Categories.OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
   
    }
}
