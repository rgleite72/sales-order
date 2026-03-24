using Microsoft.AspNetCore.Mvc;
using SalesOrder.Application.Contracts.Integration;

namespace SalesOrder.Api.Controllers;

[ApiController]
[Route("api/integration-test")]
public class IntegrationTestController : ControllerBase
{
    private readonly IProductCatalogGateway _productCatalogGateway;

    public IntegrationTestController(IProductCatalogGateway productCatalogGateway)
    {
        _productCatalogGateway = productCatalogGateway;
    }

    [HttpGet("products/{productId:guid}")]
    public async Task<IActionResult> GetProduct(Guid productId, CancellationToken ct)
    {
        var product = await _productCatalogGateway.GetProductByIdAsync(productId, ct);

        if (product is null)
        {
            return NotFound();
        }

        return Ok(product);
    }
}
