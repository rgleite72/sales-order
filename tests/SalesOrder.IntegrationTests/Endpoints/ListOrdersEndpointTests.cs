using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SalesOrder.Application.DTOs.Order;
using SalesOrder.IntegrationTests.Common;

namespace SalesOrder.IntegrationTests.Endpoints;

public class ListOrdersEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ListOrdersEndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task List_ShouldReturnPagedOrders()
    {
        await _factory.ResetDatabaseAsync();

        await _factory.SeedOrderAsync(
            IntegrationTestData.CreateOrderEntity(orderNumber: "SO-TEST-001", status: "Pending"));

        await _factory.SeedOrderAsync(
            IntegrationTestData.CreateOrderEntity(orderNumber: "SO-TEST-002", status: "Confirmed"));

        var response = await _client.GetAsync("/api/orders?page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<PagedResponseDto<SalesOrderResponseDto>>();

        body.Should().NotBeNull();
        body!.Items.Should().HaveCount(2);
        body.Total.Should().Be(2);
        body.Page.Should().Be(1);
        body.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task List_ShouldFilterByStatus()
    {
        await _factory.ResetDatabaseAsync();

        await _factory.SeedOrderAsync(
            IntegrationTestData.CreateOrderEntity(orderNumber: "SO-TEST-001", status: "Pending"));

        await _factory.SeedOrderAsync(
            IntegrationTestData.CreateOrderEntity(orderNumber: "SO-TEST-002", status: "Confirmed"));

        var response = await _client.GetAsync("/api/orders?page=1&pageSize=10&status=Confirmed");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<PagedResponseDto<SalesOrderResponseDto>>();

        body.Should().NotBeNull();
        body!.Items.Should().HaveCount(1);
        body.Items[0].Status.Should().Be("Confirmed");
        body.Total.Should().Be(1);
    }
}
