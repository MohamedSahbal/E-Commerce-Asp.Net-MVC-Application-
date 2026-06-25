using ECommerceApp.Services;
using ECommerceApplication.Data;
using ECommerceApplication.Models;
using ECommerceApplication.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ECommerce_Application.Services
{
    public class ProductImageService : IProductImageService
    {
        private readonly ApplicationDbContext _context;
        private readonly CloudinaryService _cloudinary;

        public ProductImageService(
            ApplicationDbContext context,
            CloudinaryService cloudinary)
        {
            _context = context;
            _cloudinary = cloudinary;
        }

        public async Task UploadImages(List<IFormFile> files, Product product)
        {
            if (files == null || !files.Any(f => f != null && f.Length > 0))
                return;

            bool hasMain = await _context.ProductImages
                .AnyAsync(i => i.ProductId == product.Id && i.IsMain);

            int uploaded = 0;

            foreach (var file in files.Where(f => f != null && f.Length > 0))
            {
                try
                {
                    var (url, publicId) = await _cloudinary.UploadImageAsync(file);

                    _context.ProductImages.Add(new ProductImage
                    {
                        ProductId = product.Id,
                        ImageUrl = url,
                        PublicId = publicId,
                        IsMain = !hasMain && uploaded == 0
                    });

                    uploaded++;
                }
                catch (Exception ex)
                {
                    continue;
                }
            }

            if (uploaded > 0)
                await _context.SaveChangesAsync();
        }

        public async Task DeleteImage(int id)
        {
            var image = await _context.ProductImages.FindAsync(id);

            if (image == null)
                return;

            await _cloudinary.DeleteImageAsync(image.PublicId);

            _context.ProductImages.Remove(image);

            await _context.SaveChangesAsync();
        }

        public async Task SetMainImage(int id, int productId)
        {
            var images = await _context.ProductImages
                .Where(i => i.ProductId == productId)
                .ToListAsync();

            foreach (var img in images)
            {
                img.IsMain = img.Id == id;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<ProductImageVM>> GetExistingImages(int productId)
        {
            return await _context.ProductImages
                  .Where(i => i.ProductId == productId)
                  .Select(i => new ProductImageVM
                  {
                      Id = i.Id,
                      ImageUrl = i.ImageUrl,
                      PublicId = i.PublicId,
                      IsMain = i.IsMain
                  })
                  .ToListAsync();
        }
    }
}
