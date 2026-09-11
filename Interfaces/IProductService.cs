using ProductCatalogApi.Models;
using ProductCatalogApi.DTOs;

namespace ProductCatalogApi.Services
{
    public interface IProductService
    {
        Task<ServiceResult<PagedResultDto<Product>>> GetProductsAsync(int page, int pageSize, string? search, int? categoryId, decimal? minPrice);
        Task<ServiceResult<Product>> GetProductByIdAsync(int id);
        Task<ServiceResult<Product>> CreateProductAsync(CreateProductDto dto);
        Task<ServiceResult<bool>> UpdateProductAsync(int id, UpdateProductDto dto);
        Task<ServiceResult<bool>> DeleteProductAsync(int id);
    }
}