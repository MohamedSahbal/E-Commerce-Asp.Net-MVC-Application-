using ECommerceApplication.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ECommerce_Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;

        public CategoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SelectList> GetCategoriesSelectList()
        {
           return new SelectList(await _context.Categories.OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
        }        

        public async Task<SelectList> GetParentCategoriesSelectList(int? excludeId)
        {
            var categories = await _context.Categories
                            .Where(c => c.ParentCategoryId == null &&
                            (excludeId == null || c.Id != excludeId))
                            .OrderBy(c => c.Name)
                            .ToListAsync();
            return new SelectList(categories, "Id", "Name");
        }
    }
}
