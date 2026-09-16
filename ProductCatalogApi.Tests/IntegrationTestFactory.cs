using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ProductCatalogApi.Data;
using ProductCatalogApi.Models;

namespace ProductCatalogApi.Tests;

public sealed class IntegrationTestFactory : WebApplicationFactory<Program>
{
	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.UseEnvironment("Testing");
		builder.UseSetting("Jwt:Key", "000000000000000000000000000000000");
		builder.UseSetting("Jwt:Issuer", "integration-test-issuer");
		builder.UseSetting("Jwt:Audience", "integration-test-audience");

		builder.ConfigureServices(services =>
		{
			services.RemoveAll<DbContextOptions<AppDbContext>>();
			services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
			services.AddDbContext<AppDbContext>(options =>
				options.UseInMemoryDatabase("ProductCatalogIntegrationTests"));

			using var serviceProvider = services.BuildServiceProvider();
			using var scope = serviceProvider.CreateScope();
			var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

			context.Database.EnsureDeleted();
			context.Database.EnsureCreated();

			var category = new Category
			{
				Name = "Electronics",
				Description = "Test category"
			};
			context.Categories.Add(category);

			var user = new User
			{
				Username = "integration-user",
				Role = "Admin"
			};
			user.PasswordHash = new PasswordHasher<User>().HashPassword(user, "Password123!");
			context.Users.Add(user);
			context.SaveChanges();

			context.Products.Add(new Product
			{
				Name = "Test Laptop",
				Price = 999.99m,
				Stock = 5,
				CategoryId = category.Id,
				CreatedAt = DateTime.UtcNow
			});
			context.SaveChanges();
		});
	}
}
