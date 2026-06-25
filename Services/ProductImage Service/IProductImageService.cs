using ECommerceApplication.Models;
using ECommerceApplication.ViewModels;

namespace ECommerce_Application.Services
{
    public interface IProductImageService
    {
            Task UploadImages(List<IFormFile> files, Product product);
            Task DeleteImage(int id);
            Task SetMainImage(int id, int productId);
            Task<List<ProductImageVM>> GetExistingImages(int productId);
    }
}
