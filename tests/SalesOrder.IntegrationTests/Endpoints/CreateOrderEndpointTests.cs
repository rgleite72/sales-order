using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SalesOrder.Application.DTOs.Order;
using SalesOrder.IntegrationTests.Common;

namespace SalesOrder.IntegrationTests.Endpoints;

public class CreateOrderEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public CreateOrderEndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_ShouldReturn201_AndPersistOrder()
    {
        await _factory.ResetDatabaseAsync();

        var product = IntegrationTestData.CreateProduct(initialPrice: 150m, currency: "BRL");
        _factory.FakeProductCatalogGateway.AddProduct(product);

        var request = IntegrationTestData.CreateOrderRequest(
            customerId: Guid.NewGuid(),
            productId: product.Id,
            quantity: 3,
            currency: "BRL");

        var response = await _client.PostAsJsonAsync("/api/orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<SalesOrderResponseDto>();

        body.Should().NotBeNull();
        body!.Id.Should().NotBeEmpty();
        body.Currency.Should().Be("BRL");
        body.Status.Should().Be("Pending");
        body.TotalAmount.Should().Be(450m);

        var orderInDb = await _factory.GetOrderByIdAsync(body.Id);

        orderInDb.Should().NotBeNull();
        orderInDb!.OrderItems.Should().HaveCount(1);
        orderInDb.TotalAmount.Should().Be(450m);
    }

    [Fact]
    public async Task Create_ShouldReturn422_WhenProductCurrencyDoesNotMatchOrderCurrency()
    {
        await _factory.ResetDatabaseAsync();

        var product = IntegrationTestData.CreateProduct(initialPrice: 200m, currency: "USD");
        _factory.FakeProductCatalogGateway.AddProduct(product);

        var request = IntegrationTestData.CreateOrderRequest(
            customerId: Guid.NewGuid(),
            productId: product.Id,
            quantity: 1,
            currency: "BRL");

        var response = await _client.PostAsJsonAsync("/api/orders", request);

        response.StatusCode.Should().Be((HttpStatusCode)422);
    }
}
