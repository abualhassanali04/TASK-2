using Microsoft.AspNetCore.Mvc;
using ProductCatalogApi.DTOs;
using ProductCatalogApi.Extensions;
using ProductCatalogApi.Services;

namespace ProductCatalogApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    // GET: api/products?page=1&pageSize=10&search=laptop&categoryId=1&minPrice=100
    [HttpGet]
    public async Task<ActionResult<PagedResultDto<ProductDto>>> GetProducts(
        int page = 1, int pageSize = 10, string? search = null, int? categoryId = null, decimal? minPrice = null)
    {
        var result = await _productService.GetProductsAsync(page, pageSize, search, categoryId, minPrice);

        var dtoResult = new PagedResultDto<ProductDto>
        {
            Items = result.Data!.Items.Select(p => p.ToDto()).ToList(),
            TotalCount = result.Data.TotalCount,
            Page = result.Data.Page,
            PageSize = result.Data.PageSize
        };

        return dtoResult;
    }

    // GET: api/products/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        var result = await _productService.GetProductByIdAsync(id);

        if (!result.Success)
            return NotFound(result.ErrorMessage);

        return result.Data!.ToDto();
    }

    // POST: api/products
    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductDto dto)
    {
        var result = await _productService.CreateProductAsync(dto);

        if (!result.Success)
            return BadRequest(result.ErrorMessage);

        var productDto = result.Data!.ToDto();
        return CreatedAtAction(nameof(GetProduct), new { id = productDto.Id }, productDto);
    }

    // PUT: api/products/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateProduct(int id, UpdateProductDto dto)
    {
        var result = await _productService.UpdateProductAsync(id, dto);

        if (!result.Success)
        {
            return result.ErrorType == ServiceErrorType.NotFound
                ? NotFound(result.ErrorMessage)
                : BadRequest(result.ErrorMessage);
        }

        return NoContent();
    }

    // DELETE: api/products/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var result = await _productService.DeleteProductAsync(id);

        if (!result.Success)
            return NotFound(result.ErrorMessage);

        return NoContent();
    }
}