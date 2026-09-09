using ProductCatalogApi.Models;
using ProductCatalogApi.DTOs;

namespace MappingExtensions;

public static class MappingExtensions
{
    // ===== Product =====

    public static ProductDto ToDto(this Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Stock = product.Stock,
            CreatedAt = product.CreatedAt,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name
        };
    }

    public static Product ToEntity(this CreateProductDto createProductDto)
    {
        return new Product
        {
            Name = createProductDto.Name,
            Price = createProductDto.Price,
            Stock = createProductDto.Stock,
            CategoryId = createProductDto.CategoryId,
            CreatedAt = DateTime.Now
        };
    }

    public static void UpdateEntity(this UpdateProductDto newProductDto, Product originalProduct)
    {
        originalProduct.Name = newProductDto.Name;
        originalProduct.Price = newProductDto.Price;
        originalProduct.Stock = newProductDto.Stock;
        originalProduct.CategoryId = newProductDto.CategoryId;
    }

    // ===== Category =====

    public static CategoryDto ToDto(this Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description ?? string.Empty
        };
    }

    public static Category ToEntity(this CreateCategoryDto createCategoryDto)
    {
        return new Category
        {
            Name = createCategoryDto.Name,
            Description = createCategoryDto.Description ?? string.Empty
        };
    }

    public static void UpdateEntity(this UpdateCategoryDto newCategoryDto, Category originalCategory)
    {
        originalCategory.Name = newCategoryDto.Name;
        originalCategory.Description = newCategoryDto.Description ?? string.Empty;
    }
}