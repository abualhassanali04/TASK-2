using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductCatalogApi.Data;
using ProductCatalogApi.Models;
using ProductCatalogApi.DTOs;
using ProductCatalogApi.Extensions;

namespace ProductCatalogApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(AppDbContext context, ILogger<ProductsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/products?page=1&pageSize=10&search=laptop&categoryId=1&minPrice=100
    [HttpGet]
    public async Task<ActionResult<PagedResultDto<ProductDto>>> GetProducts(
        int page = 1,
        int pageSize = 10,
        string? search = null,
        int? categoryId = null,
        decimal? minPrice = null)
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

        return new PagedResultDto<ProductDto>
        {
            Items = products.Select(p => p.ToDto()).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // GET: api/products/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
        {
            _logger.LogWarning("Product with id {Id} was not found", id);
            return NotFound("Product not found.");
        }

        return product.ToDto();
    }

    // POST: api/products
    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductDto dto)
    {
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
        if (!categoryExists)
        {
            _logger.LogWarning("Attempted to create product with invalid CategoryId {CategoryId}", dto.CategoryId);
            return BadRequest("Invalid CategoryId.");
        }

        var product = dto.ToEntity();

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Product {Id} ({Name}) created under category {CategoryId}", product.Id, product.Name, product.CategoryId);

        // نحمّل التصنيف عشان الـ CategoryName تظهر صح بالرد
        await _context.Entry(product).Reference(p => p.Category).LoadAsync();

        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product.ToDto());
    }

    // PUT: api/products/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateProduct(int id, UpdateProductDto dto)
    {
        var existingProduct = await _context.Products.FindAsync(id);
        if (existingProduct == null)
        {
            _logger.LogWarning("Attempted to update non-existent product with id {Id}", id);
            return NotFound("Product not found.");
        }

        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
        if (!categoryExists)
        {
            _logger.LogWarning("Attempted to update product {Id} with invalid CategoryId {CategoryId}", id, dto.CategoryId);
            return BadRequest("Invalid CategoryId.");
        }

        dto.UpdateEntity(existingProduct);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Product {Id} was updated", id);

        return NoContent();
    }

    // DELETE: api/products/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            _logger.LogWarning("Attempted to delete non-existent product with id {Id}", id);
            return NotFound("Product not found.");
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Product {Id} ({Name}) was deleted", product.Id, product.Name);

        return NoContent();
    }
}