using SalesOrder.Application.Contracts.Integration;
using SalesOrder.Application.DTOs.Integration;

namespace SalesOrder.IntegrationTests.Common;

public class FakeProductCatalogGateway : IProductCatalogGateway
{
    private readonly Dictionary<Guid, ProductCatalogProductDto> _products = new();

    public void AddProduct(ProductCatalogProductDto product)
    {
        _products[product.Id] = product;
    }

    public void Clear()
    {
        _products.Clear();
    }

    public Task<ProductCatalogProductDto?> GetProductByIdAsync(Guid productId, CancellationToken ct)
    {
        _products.TryGetValue(productId, out var product);
        return Task.FromResult(product);
    }
}
