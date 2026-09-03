using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductCatalogApi.Data;
using ProductCatalogApi.Models;
using ProductCatalogApi.DTOs;

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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
    {
        return await _context.Products
            .Include(p => p.Category)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Stock = p.Stock,
                CreatedAt = p.CreatedAt,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name
            })
            .ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Where(p => p.Id == id)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Stock = p.Stock,
                CreatedAt = p.CreatedAt,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name
            })
            .FirstOrDefaultAsync();

        if (product == null)
        {
            _logger.LogWarning("Product with id {Id} was not found", id);
            return NotFound("Product not found.");
        }

        return product;
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductDto dto)
    {
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
        if (!categoryExists)
        {
            _logger.LogWarning("Attempted to create product with invalid CategoryId {CategoryId}", dto.CategoryId);
            return BadRequest("Invalid CategoryId.");
        }

        var product = new Product
        {
            Name = dto.Name,
            Price = dto.Price,
            Stock = dto.Stock,
            CategoryId = dto.CategoryId,
            CreatedAt = DateTime.Now
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Product {Id} ({Name}) created under category {CategoryId}", product.Id, product.Name, product.CategoryId);

        var category = await _context.Categories.FindAsync(dto.CategoryId);

        var resultDto = new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Stock = product.Stock,
            CreatedAt = product.CreatedAt,
            CategoryId = product.CategoryId,
            CategoryName = category?.Name
        };

        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, resultDto);
    }

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

        existingProduct.Name = dto.Name;
        existingProduct.Price = dto.Price;
        existingProduct.Stock = dto.Stock;
        existingProduct.CategoryId = dto.CategoryId;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Product {Id} was updated", id);

        return NoContent();
    }

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