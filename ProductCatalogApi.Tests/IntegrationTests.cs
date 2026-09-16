using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ProductCatalogApi.DTOs;

namespace ProductCatalogApi.Tests;

public sealed class IntegrationTests : IClassFixture<IntegrationTestFactory>
{
    private readonly HttpClient _client;

    public IntegrationTests(IntegrationTestFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetProducts_ReturnsSeededProduct()
    {
        var response = await _client.GetAsync("/api/products");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<ProductDto>>();

        Assert.NotNull(result);
        Assert.Contains(result!.Items, product => product.Name == "Test Laptop");
    }

    [Fact]
    public async Task CreateProduct_WithoutAuthentication_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/products", new
        {
            name = "Unauthorized Product",
            price = 10.00m,
            stock = 1,
            categoryId = 1
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task LoginThenCreateCategory_WithAdminToken_ReturnsCreated()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            username = "integration-user",
            password = "Password123!"
        });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        using var loginJson = JsonDocument.Parse(await loginResponse.Content.ReadAsStringAsync());
        var accessToken = loginJson.RootElement.GetProperty("accessToken").GetString();
        Assert.False(string.IsNullOrWhiteSpace(accessToken));

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.PostAsJsonAsync("/api/categories", new
        {
            name = "Office Gear",
            description = "Integration test category"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}