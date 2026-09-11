using Microsoft.AspNetCore.Mvc;
using ProductCatalogApi.DTOs;
using ProductCatalogApi.Services;

namespace ProductCatalogApi.Controllers
{
    [ApiController]
    [Route("api/reports")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("inventory-value")]
        public async Task<ActionResult<InventoryValueDto>> GetInventoryValue()
        {
            var result = await _reportService.GetTotalInventoryValueAsync();
            return result.Data!;
        }

        [HttpGet("most-expensive-product")]
        public async Task<ActionResult<MostExpensiveProductDto>> GetMostExpensiveProduct()
        {
            var result = await _reportService.GetMostExpensiveProductAsync();

            if (!result.Success)
                return NotFound(result.ErrorMessage);

            return result.Data!;
        }

        [HttpGet("out-of-stock")]
        public async Task<ActionResult<IEnumerable<OutOfStockProductDto>>> GetOutOfStockProducts()
        {
            var result = await _reportService.GetOutOfStockProductsAsync();
            return result.Data!.ToList();
        }
    }
}