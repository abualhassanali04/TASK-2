using ProductCatalogApi.Models;
using ProductCatalogApi.DTOs;

namespace ProductCatalogApi.Services
{
    public interface ICategoryService
    {
        Task<ServiceResult<IEnumerable<Category>>> GetAllCategoriesAsync();
        Task<ServiceResult<Category>> GetCategoryByIdAsync(int id);
        Task<ServiceResult<Category>> CreateCategoryAsync(CreateCategoryDto dto);
        Task<ServiceResult<bool>> UpdateCategoryAsync(int id, UpdateCategoryDto dto);
        Task<ServiceResult<bool>> DeleteCategoryAsync(int id);
        Task<ServiceResult<IEnumerable<Product>>> GetProductsByCategoryIdAsync(int categoryId);
    }
}