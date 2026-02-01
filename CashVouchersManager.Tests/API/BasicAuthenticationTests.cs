using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using CashVouchersManager.DTO.DTOs;

namespace CashVouchersManager.Tests.API;

/// <summary>
/// Integration tests for Basic Authentication
/// </summary>
public class BasicAuthenticationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    /// <summary>
    /// Initializes a new instance of the BasicAuthenticationTests class
    /// </summary>
    /// <param name="factory">The web application factory</param>
    public BasicAuthenticationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["AppSettings:Port"] = "5000",
                    ["AppSettings:UseHttpsRedirection"] = "false",
                    ["AppSettings:Authentication:Username"] = "testuser",
                    ["AppSettings:Authentication:Password"] = "testpass"
                });
            });
        });

        _client = _factory.CreateClient();
    }

    /// <summary>
    /// Tests that requests without authentication return 401
    /// </summary>
    [Fact]
    public async Task Request_WithoutAuthentication_ShouldReturn401()
    {
        // Act
        var response = await _client.GetAsync("/api/GetFilteredCashVouchers");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.True(response.Headers.Contains("WWW-Authenticate"));
    }

    /// <summary>
    /// Tests that requests with invalid credentials return 401
    /// </summary>
    [Fact]
    public async Task Request_WithInvalidCredentials_ShouldReturn401()
    {
        // Arrange
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes("wronguser:wrongpass"));
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        // Act
        var response = await _client.GetAsync("/api/GetFilteredCashVouchers");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Tests that requests with valid credentials succeed
    /// </summary>
    [Fact]
    public async Task Request_WithValidCredentials_ShouldSucceed()
    {
        // Arrange
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes("testuser:testpass"));
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        // Act
        var response = await _client.GetAsync("/api/GetFilteredCashVouchers");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// Tests that Swagger endpoints do not require authentication
    /// </summary>
    [Fact]
    public async Task SwaggerEndpoint_WithoutAuthentication_ShouldSucceed()
    {
        // Act
        var response = await _client.GetAsync("/swagger/index.html");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// Tests that POST requests with valid credentials succeed
    /// </summary>
    [Fact]
    public async Task PostRequest_WithValidCredentials_ShouldSucceed()
    {
        // Arrange
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes("testuser:testpass"));
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        var request = new GenerateCashVoucherRequestDTO
        {
            Amount = 100.00m,
            IssuingStoreId = 5678
        };

        var content = new StringContent(
            System.Text.Json.JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/GenerateCashVoucher", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// Tests that POST requests without authentication return 401
    /// </summary>
    [Fact]
    public async Task PostRequest_WithoutAuthentication_ShouldReturn401()
    {
        // Arrange
        var request = new GenerateCashVoucherRequestDTO
        {
            Amount = 100.00m,
            IssuingStoreId = 5678
        };

        var content = new StringContent(
            System.Text.Json.JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/GenerateCashVoucher", content);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
