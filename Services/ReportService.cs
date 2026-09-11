using Microsoft.EntityFrameworkCore;
using ProductCatalogApi.Data;
using ProductCatalogApi.DTOs;

namespace ProductCatalogApi.Services
{
    public class ReportService : IReportService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ReportService> _logger;

        public ReportService(AppDbContext context, ILogger<ReportService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ServiceResult<InventoryValueDto>> GetTotalInventoryValueAsync()
        {
            var totalValue = await _context.Products.SumAsync(p => p.Price * p.Stock);
            var roundedTotalValue = Math.Round(totalValue, 2);

            _logger.LogInformation("Inventory value report generated: {TotalValue}", totalValue);

            return ServiceResult<InventoryValueDto>.Ok(new InventoryValueDto { TotalValue = roundedTotalValue });
        }

        public async Task<ServiceResult<MostExpensiveProductDto>> GetMostExpensiveProductAsync()
        {
            var product = await _context.Products
                .OrderByDescending(p => p.Price)
                .Select(p => new MostExpensiveProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price
                })
                .FirstOrDefaultAsync();

            if (product == null)
            {
                _logger.LogWarning("Most expensive product report requested but no products exist");
                return ServiceResult<MostExpensiveProductDto>.Fail("No products available.", ServiceErrorType.NotFound);
            }

            return ServiceResult<MostExpensiveProductDto>.Ok(product);
        }

        public async Task<ServiceResult<IEnumerable<OutOfStockProductDto>>> GetOutOfStockProductsAsync()
        {
            var products = await _context.Products
                .Where(p => p.Stock == 0)
                .Select(p => new OutOfStockProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Stock = p.Stock
                })
                .ToListAsync();

            _logger.LogInformation("Out-of-stock report generated: {Count} products", products.Count);

            return ServiceResult<IEnumerable<OutOfStockProductDto>>.Ok(products);
        }
    }
}