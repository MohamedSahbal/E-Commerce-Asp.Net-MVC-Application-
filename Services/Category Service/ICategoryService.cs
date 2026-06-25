using Microsoft.AspNetCore.Mvc.Rendering;

namespace ECommerce_Application.Services
{
    public interface ICategoryService
    {
        Task<SelectList> GetCategoriesSelectList();
        Task<SelectList> GetParentCategoriesSelectList(int? excludeId);
    }
}
