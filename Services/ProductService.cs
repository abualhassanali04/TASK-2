using Microsoft.EntityFrameworkCore;
using ProductCatalogApi.Data;
using ProductCatalogApi.Models;
using ProductCatalogApi.DTOs;
using ProductCatalogApi.Extensions;

namespace ProductCatalogApi.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProductService> _logger;

        public ProductService(AppDbContext context, ILogger<ProductService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ServiceResult<PagedResultDto<Product>>> GetProductsAsync(
            int page, int pageSize, string? search, int? categoryId, decimal? minPrice)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var query = _context.Products.Include(p => p.Category).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.Name.ToLower().Contains(search.ToLower()));

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);

            var totalCount = await query.CountAsync();

            var products = await query
                .OrderBy(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new PagedResultDto<Product>
            {
                Items = products,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return ServiceResult<PagedResultDto<Product>>.Ok(result);
        }

        public async Task<ServiceResult<Product>> GetProductByIdAsync(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                _logger.LogWarning("Product {Id} was requested but not found", id);
                return ServiceResult<Product>.Fail("Product not found.", ServiceErrorType.NotFound);
            }

            return ServiceResult<Product>.Ok(product);
        }

        public async Task<ServiceResult<Product>> CreateProductAsync(CreateProductDto dto)
        {
            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
            if (!categoryExists)
            {
                _logger.LogWarning("Attempted to create product with invalid CategoryId {CategoryId}", dto.CategoryId);
                return ServiceResult<Product>.Fail("Invalid CategoryId.", ServiceErrorType.ValidationError);
            }

            var product = dto.ToEntity();

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            await _context.Entry(product).Reference(p => p.Category).LoadAsync();

            _logger.LogInformation("Product {Id} ({Name}) created under category {CategoryId}", product.Id, product.Name, product.CategoryId);

            return ServiceResult<Product>.Ok(product);
        }

        public async Task<ServiceResult<bool>> UpdateProductAsync(int id, UpdateProductDto dto)
        {
            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null)
            {
                _logger.LogWarning("Attempted to update non-existent product with id {Id}", id);
                return ServiceResult<bool>.Fail("Product not found.", ServiceErrorType.NotFound);
            }

            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
            if (!categoryExists)
            {
                _logger.LogWarning("Attempted to update product {Id} with invalid CategoryId {CategoryId}", id, dto.CategoryId);
                return ServiceResult<bool>.Fail("Invalid CategoryId.", ServiceErrorType.ValidationError);
            }

            dto.UpdateEntity(existingProduct);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Product {Id} was updated", id);

            return ServiceResult<bool>.Ok(true);
        }

        public async Task<ServiceResult<bool>> DeleteProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                _logger.LogWarning("Attempted to delete non-existent product with id {Id}", id);
                return ServiceResult<bool>.Fail("Product not found.", ServiceErrorType.NotFound);
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Product {Id} ({Name}) was deleted", product.Id, product.Name);

            return ServiceResult<bool>.Ok(true);
        }
    }
}