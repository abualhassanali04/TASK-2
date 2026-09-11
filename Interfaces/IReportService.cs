using ProductCatalogApi.DTOs;

namespace ProductCatalogApi.Services
{
    public interface IReportService
    {
        Task<ServiceResult<InventoryValueDto>> GetTotalInventoryValueAsync();
        Task<ServiceResult<MostExpensiveProductDto>> GetMostExpensiveProductAsync();
        Task<ServiceResult<IEnumerable<OutOfStockProductDto>>> GetOutOfStockProductsAsync();
    }
}