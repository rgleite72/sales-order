using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using SalesOrder.Application.Contracts.Integration;
using SalesOrder.Application.DTOs.Integration;

namespace SalesOrder.Infrastructure.Integrations.ProductCatalog;

public class ProductCatalogGateway : IProductCatalogGateway
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductCatalogGateway> _logger;

    public ProductCatalogGateway(
        HttpClient httpClient,
        ILogger<ProductCatalogGateway> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ProductCatalogProductDto?> GetProductByIdAsync(Guid productId, CancellationToken ct)
    {
        _logger.LogInformation(
            "Calling ProductCatalog API. ProductId={ProductId}",
            productId);

        var response = await _httpClient.GetAsync($"/api/products/{productId}", ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning(
                "Product not found in ProductCatalog. ProductId={ProductId}",
                productId);

            return null;
        }

        response.EnsureSuccessStatusCode();

        var product = await response.Content.ReadFromJsonAsync<ProductCatalogProductDto>(cancellationToken: ct);

        _logger.LogInformation(
            "Product retrieved successfully from ProductCatalog. ProductId={ProductId}",
            productId);

        return product;
    }
}
