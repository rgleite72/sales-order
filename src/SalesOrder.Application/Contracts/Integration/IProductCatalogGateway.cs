using SalesOrder.Application.DTOs.Integration;

namespace SalesOrder.Application.Contracts.Integration;

public interface IProductCatalogGateway
{
    Task<ProductCatalogProductDto?> GetProductByIdAsync(Guid productId, CancellationToken ct);
}


