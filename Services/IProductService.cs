using ProductCatalogApi.Models;
using ProductCatalogApi.DTOs;

namespace ProductCatalogApi.Services
{
    public interface IProductService
    {
        Task<PagedResultDto<Product>> GetProductsAsync(int page, int pageSize, string? search, int? categoryId, decimal? minPrice);
        Task<Product?> GetProductByIdAsync(int id);
        Task<Product> CreateProductAsync(CreateProductDto dto);
        Task<bool> UpdateProductAsync(int id, UpdateProductDto dto);
        Task<bool> DeleteProductAsync(int id);
        Task<bool> CategoryExistsAsync(int categoryId);
    }
}