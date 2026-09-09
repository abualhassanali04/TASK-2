using ProductCatalogApi.Models;
using ProductCatalogApi.DTOs;

namespace ProductCatalogApi.Services
{
    public interface IReportService
    {
        Task<InventoryValueDto> GetTotalInventoryValueAsync();
        Task<MostExpensiveProductDto?> GetMostExpensiveProductAsync();
        Task<IEnumerable<OutOfStockProductDto>> GetOutOfStockProductsAsync();
    }
}