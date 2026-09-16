using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using ProductCatalogApi.Data;
using ProductCatalogApi.DTOs;
using ProductCatalogApi.Models;
using ProductCatalogApi.Services;
using Xunit;

namespace ProductCatalogApi.Tests;

public class AuthServiceTest
{
    [Fact]
    public async Task LoginAsync_WithCorrectPassword_Succeeds()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDbContext(options);
        var passwordHasher = new PasswordHasher<User>();
        var user = new User
        {
            Id = 1,
            Username = "test",
            Role = "User"
        };
        user.PasswordHash = passwordHasher.HashPassword(user, "12345678");
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "00000000000000000000000000000000",
                ["Jwt:Issuer"] = "test-issuer",
                ["Jwt:Audience"] = "test-audience"
            })
            .Build();
        var service = new AuthService(
            context,
            configuration,
            NullLogger<AuthService>.Instance);
        var request = new LoginDto
        {
            Username = "test",
            Password = "12345678"
        };

        var result = await service.LoginAsync(request);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.False(string.IsNullOrWhiteSpace(result.Data!.AccessToken));
    }
}