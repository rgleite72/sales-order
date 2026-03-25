using System.Net;
using FluentAssertions;
using SalesOrder.IntegrationTests.Common;

namespace SalesOrder.IntegrationTests.Endpoints;

public class CancelOrderEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public CancelOrderEndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Cancel_ShouldReturn204_AndUpdateStatusToCanceled()
    {
        await _factory.ResetDatabaseAsync();

        var order = IntegrationTestData.CreateOrderEntity(status: "Pending");
        await _factory.SeedOrderAsync(order);

        var response = await _client.PatchAsync($"/api/orders/{order.Id}/cancel", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var orderInDb = await _factory.GetOrderByIdAsync(order.Id);

        orderInDb.Should().NotBeNull();
        orderInDb!.Status.Should().Be("Canceled");
    }

    [Fact]
    public async Task Cancel_ShouldReturn422_WhenOrderIsAlreadyCanceled()
    {
        await _factory.ResetDatabaseAsync();

        var order = IntegrationTestData.CreateOrderEntity(status: "Canceled");
        await _factory.SeedOrderAsync(order);

        var response = await _client.PatchAsync($"/api/orders/{order.Id}/cancel", content: null);

        response.StatusCode.Should().Be((HttpStatusCode)422);
    }
}
