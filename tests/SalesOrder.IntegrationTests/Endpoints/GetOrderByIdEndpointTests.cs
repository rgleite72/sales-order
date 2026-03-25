using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SalesOrder.Application.DTOs.Order;
using SalesOrder.IntegrationTests.Common;

namespace SalesOrder.IntegrationTests.Endpoints;

public class GetOrderByIdEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public GetOrderByIdEndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetById_ShouldReturn200_WhenOrderExists()
    {
        await _factory.ResetDatabaseAsync();

        var order = IntegrationTestData.CreateOrderEntity();
        await _factory.SeedOrderAsync(order);

        var response = await _client.GetAsync($"/api/orders/{order.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<SalesOrderResponseDto>();

        body.Should().NotBeNull();
        body!.Id.Should().Be(order.Id);
        body.OrderNumber.Should().Be(order.OrderNumber);
        body.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetById_ShouldReturn404_WhenOrderDoesNotExist()
    {
        await _factory.ResetDatabaseAsync();

        var response = await _client.GetAsync($"/api/orders/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
