using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductCatalogApi.Data;
using ProductCatalogApi.DTOs;

namespace ProductCatalogApi.Controllers
{
    [ApiController]
    [Route("api/reports")]
    public class ReportsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(AppDbContext context, ILogger<ReportsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/reports/inventory-value
        [HttpGet("inventory-value")]
        public async Task<ActionResult<InventoryValueDto>> GetInventoryValue()
        {
            var totalValue = await _context.Products
                .SumAsync(p => p.Price * p.Stock);

            _logger.LogInformation("Inventory value report generated: {TotalValue}", totalValue);

            return new InventoryValueDto { TotalValue = totalValue };
        }

        // GET: api/reports/most-expensive-product
        [HttpGet("most-expensive-product")]
        public async Task<ActionResult<MostExpensiveProductDto>> GetMostExpensiveProduct()
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
                return NotFound("No products available.");
            }

            return product;
        }

        // GET: api/reports/out-of-stock
        [HttpGet("out-of-stock")]
        public async Task<ActionResult<IEnumerable<OutOfStockProductDto>>> GetOutOfStockProducts()
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

            return products;
        }

    }
}