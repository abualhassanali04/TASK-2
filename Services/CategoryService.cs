using Microsoft.EntityFrameworkCore;
using ProductCatalogApi.Data;
using ProductCatalogApi.Models;
using ProductCatalogApi.DTOs;
using ProductCatalogApi.Extensions;

namespace ProductCatalogApi.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(AppDbContext context, ILogger<CategoryService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ServiceResult<IEnumerable<Category>>> GetAllCategoriesAsync()
        {
            var categories = await _context.Categories.ToListAsync();
            return ServiceResult<IEnumerable<Category>>.Ok(categories);
        }

        public async Task<ServiceResult<Category>> GetCategoryByIdAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                _logger.LogWarning("Category {Id} was requested but not found", id);
                return ServiceResult<Category>.Fail("Category not found.", ServiceErrorType.NotFound);
            }

            return ServiceResult<Category>.Ok(category);
        }

        public async Task<ServiceResult<Category>> CreateCategoryAsync(CreateCategoryDto dto)
        {
            var category = dto.ToEntity();

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Category {Id} ({Name}) created", category.Id, category.Name);

            return ServiceResult<Category>.Ok(category);
        }

        public async Task<ServiceResult<bool>> UpdateCategoryAsync(int id, UpdateCategoryDto dto)
        {
            var existingCategory = await _context.Categories.FindAsync(id);
            if (existingCategory == null)
            {
                _logger.LogWarning("Attempted to update non-existent category with id {Id}", id);
                return ServiceResult<bool>.Fail("Category not found.", ServiceErrorType.NotFound);
            }

            dto.UpdateEntity(existingCategory);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Category {Id} was updated", id);

            return ServiceResult<bool>.Ok(true);
        }

        public async Task<ServiceResult<bool>> DeleteCategoryAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                _logger.LogWarning("Attempted to delete non-existent category with id {Id}", id);
                return ServiceResult<bool>.Fail("Category not found.", ServiceErrorType.NotFound);
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Category {Id} ({Name}) was deleted", category.Id, category.Name);

            return ServiceResult<bool>.Ok(true);
        }

        public async Task<ServiceResult<IEnumerable<Product>>> GetProductsByCategoryIdAsync(int categoryId)
        {
            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == categoryId);
            if (!categoryExists)
            {
                _logger.LogWarning("Attempted to get products for non-existent category {Id}", categoryId);
                return ServiceResult<IEnumerable<Product>>.Fail($"Category with id {categoryId} not found.", ServiceErrorType.NotFound);
            }

            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.CategoryId == categoryId)
                .ToListAsync();

            return ServiceResult<IEnumerable<Product>>.Ok(products);
        }
    }
}