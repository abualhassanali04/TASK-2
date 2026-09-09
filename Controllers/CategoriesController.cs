using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductCatalogApi.Data;
using ProductCatalogApi.Models;
using ProductCatalogApi.DTOs;
using MappingExtensions;

namespace ProductCatalogApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(AppDbContext context, ILogger<CategoriesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/categories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
        {
            var categories = await _context.Categories.ToListAsync();
            return categories.Select(c => c.ToDto()).ToList();
        }

        // GET: api/categories/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<CategoryDto>> GetCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                _logger.LogWarning("Category with id {Id} was not found", id);
                return NotFound("Category not found.");
            }
            return category.ToDto();
        }

        // GET: api/categories/5/products
        [HttpGet("{id}/products")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByCategory(int id)
        {
            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == id);
            if (!categoryExists)
            {
                _logger.LogWarning("Attempted to get products for non-existent category {Id}", id);
                return NotFound($"Category with id {id} not found.");
            }

            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.CategoryId == id)
                .ToListAsync();

            return products.Select(p => p.ToDto()).ToList();
        }

        // POST: api/categories
        [HttpPost]
        public async Task<ActionResult<CategoryDto>> CreateCategory(CreateCategoryDto dto)
        {
            var category = dto.ToEntity();

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Category {Id} ({Name}) created", category.Id, category.Name);

            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category.ToDto());
        }

        // PUT: api/categories/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateCategory(int id, UpdateCategoryDto dto)
        {
            var existingCategory = await _context.Categories.FindAsync(id);
            if (existingCategory == null)
            {
                _logger.LogWarning("Attempted to update non-existent category with id {Id}", id);
                return NotFound("Category not found.");
            }

            dto.UpdateEntity(existingCategory);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Category {Id} was updated", id);

            return NoContent();
        }

        // DELETE: api/categories/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                _logger.LogWarning("Attempted to delete non-existent category with id {Id}", id);
                return NotFound("Category not found.");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Category {Id} ({Name}) was deleted", category.Id, category.Name);

            return NoContent();
        }
    }
}