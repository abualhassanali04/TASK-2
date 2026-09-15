using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ProductCatalogApi.Data;
using ProductCatalogApi.DTOs;
using ProductCatalogApi.Models;
using ProductCatalogApi.Services;
using Xunit;

namespace ProductCatalogApi.Tests;

public class ProductsServiceTests
{
    [Fact]
    public async Task CreateProductAsync_WithValidCategory_Succeeds()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDbContext(options);
        context.Categories.Add(new Category { Id = 1, Name = "Electronics", Description = string.Empty });
        await context.SaveChangesAsync();

        var service = new ProductService(context, NullLogger<ProductService>.Instance);
        var dto = new CreateProductDto
        {
            Name = "Laptop",
            Price = 999.99m,
            Stock = 5,
            CategoryId = 1
        };

        var result = await service.CreateProductAsync(dto);

        Assert.True(result.Success);
        Assert.Equal("Laptop", result.Data!.Name);
        Assert.Equal(1, await context.Products.CountAsync());
    }
    [Fact]
    public async Task CreateProductAsync_WithInvalidCategory_Fails()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDbContext(options);
        var service = new ProductService(context, NullLogger<ProductService>.Instance);
        var dto = new CreateProductDto
        {
            Name = "Laptop",
            Price = 999.99m,
            Stock = 5,
            CategoryId = 999
        };

        var result = await service.CreateProductAsync(dto);

        Assert.False(result.Success);
        Assert.Equal("Invalid CategoryId.", result.ErrorMessage);
    }
    [Fact]
    public async Task UpdateProductAsync_WithValidCategory_Succeeds()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDbContext(options);
        var service = new ProductService(context, NullLogger<ProductService>.Instance);
        context.Categories.Add(new Category { Id = 1, Name = "Electronics", Description = string.Empty });
        context.Products.Add(new Product { Id = 1, Name = "Old Laptop", Price = 899.99m, Stock = 3, CategoryId = 1 });
        await context.SaveChangesAsync();

        var UpdateDto = new UpdateProductDto
        {
            Name = "New Laptop",
            Price = 999.99m,
            Stock = 5,
            CategoryId = 1
        };

        var result = await service.UpdateProductAsync(1, UpdateDto);
        await context.SaveChangesAsync();

        Assert.True(result.Success);
        Assert.Equal("New Laptop", context.Products.First().Name);
    }
    [Fact]
    public async Task UpdateProductAsync_WithInvalidCategory_Fails()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDbContext(options);
        var service = new ProductService(context, NullLogger<ProductService>.Instance);
        context.Categories.Add(new Category { Id = 1, Name = "Electronics", Description = string.Empty });
        context.Products.Add(new Product { Id = 1, Name = "Old Laptop", Price = 899.99m, Stock = 3, CategoryId = 1 });
        await context.SaveChangesAsync();

        var UpdateDto = new UpdateProductDto
        {
            Name = "New Laptop",
            Price = 999.99m,
            Stock = 5,
            CategoryId = 999
        };

        var result = await service.UpdateProductAsync(1, UpdateDto);

        Assert.False(result.Success);
        Assert.Equal("Invalid CategoryId.", result.ErrorMessage);
    }
    [Fact]
    public async Task UpdateProductAsync_WithNonExistentProduct_Fails()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDbContext(options);
        var service = new ProductService(context, NullLogger<ProductService>.Instance);
        context.Categories.Add(new Category { Id = 1, Name = "Electronics", Description = string.Empty });
        await context.SaveChangesAsync();

        var UpdateDto = new UpdateProductDto
        {
            Name = "New Laptop",
            Price = 999.99m,
            Stock = 5,
            CategoryId = 1
        };

        var result = await service.UpdateProductAsync(999, UpdateDto);

        Assert.False(result.Success);
        Assert.Equal("Product not found.", result.ErrorMessage);
    }
    [Fact]
    public async Task DeleteProductAsync_WithExistingProduct_Succeeds()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDbContext(options);
        var service = new ProductService(context, NullLogger<ProductService>.Instance);
        context.Categories.Add(new Category { Id = 1, Name = "Electronics", Description = string.Empty });
        context.Products.Add(new Product { Id = 1, Name = "Laptop", Price = 999.99m, Stock = 5, CategoryId = 1 });
        await context.SaveChangesAsync();

        var result = await service.DeleteProductAsync(1);

        Assert.True(result.Success);
        Assert.Equal(0, await context.Products.CountAsync());
    }
    [Fact]
    public async Task DeleteProductAsync_WithNonExistentProduct_Fails()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDbContext(options);
        var service = new ProductService(context, NullLogger<ProductService>.Instance);

        var result = await service.DeleteProductAsync(999);

        Assert.False(result.Success);
        Assert.Equal("Product not found.", result.ErrorMessage);
    }
    [Fact]
    public async Task GetProductAsync_WithExistingProduct_Succeeds()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDbContext(options);
        var service = new ProductService(context, NullLogger<ProductService>.Instance);
        context.Categories.Add(new Category { Id = 1, Name = "Electronics", Description = string.Empty });
        context.Products.Add(new Product { Id = 1, Name = "Laptop", Price = 999.99m, Stock = 5, CategoryId = 1 });
        await context.SaveChangesAsync();

        var result = await service.GetProductByIdAsync(1);

        Assert.True(result.Success);
        Assert.Equal("Laptop", result.Data!.Name);
    }
    [Fact]
    public async Task GetProductAsync_WithNonExistentProduct_Fails()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDbContext(options);
        var service = new ProductService(context, NullLogger<ProductService>.Instance);

        var result = await service.GetProductByIdAsync(999);

        Assert.False(result.Success);
        Assert.Equal("Product not found.", result.ErrorMessage);
    }
    [Fact]
    public async Task GetProductsAsync_ReturnsAllProducts()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDbContext(options);
        var service = new ProductService(context, NullLogger<ProductService>.Instance);
        context.Categories.Add(new Category { Id = 1, Name = "Electronics", Description = string.Empty });
        context.Products.Add(new Product { Id = 1, Name = "Laptop", Price = 999.99m, Stock = 5, CategoryId = 1 });
        context.Products.Add(new Product { Id = 2, Name = "Smartphone", Price = 499.99m, Stock = 10, CategoryId = 1 });
        await context.SaveChangesAsync();

        var result = await service.GetProductsAsync(1, 10, null, null, null);

        Assert.True(result.Success);
        Assert.Equal(2, result.Data!.Items.Count);
    }
}