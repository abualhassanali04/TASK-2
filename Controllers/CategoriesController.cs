using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductCatalogApi.Data;
using ProductCatalogApi.Models;
using ProductCatalogApi.DTOs;

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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
        {
            return await _context.Categories
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description
                })
                .ToListAsync();
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<CategoryDto>> GetCategory(int id)
        {
            var category = await _context.Categories
                .Where(c => c.Id == id)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description
                })
                .FirstOrDefaultAsync();

            if (category == null)
            {
                _logger.LogWarning("Category with id {Id} was not found", id);
                return NotFound("Category not found.");
            }
            return category;
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
                .Where(p => p.CategoryId == id)
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

            return products;
        }

        [HttpPost]
        public async Task<ActionResult<CategoryDto>> CreateCategory(CreateCategoryDto dto)
        {
            var category = new Category
            {
                Name = dto.Name,
                Description = dto.Description
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Category {Id} ({Name}) created", category.Id, category.Name);

            var resultDto = new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };

            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, resultDto);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateCategory(int id, UpdateCategoryDto dto)
        {
            var existingCategory = await _context.Categories.FindAsync(id);
            if (existingCategory == null)
            {
                _logger.LogWarning("Attempted to update non-existent category with id {Id}", id);
                return NotFound("Category not found.");
            }

            existingCategory.Name = dto.Name;
            existingCategory.Description = dto.Description;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Category {Id} was updated", id);

            return NoContent();
        }

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