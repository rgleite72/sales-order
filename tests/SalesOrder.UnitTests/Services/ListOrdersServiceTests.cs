using SalesOrder.Application.Services.Orders;
using SalesOrder.UnitTests.Common;
using Moq;

namespace SalesOrder.UnitTests.Services;

public sealed class ListOrdersServiceTests
{
    private readonly OrderServiceTestFixture _fixture;
    private readonly OrderService _service;

    public ListOrdersServiceTests()
    {
        _fixture = new OrderServiceTestFixture();
        _service = _fixture.CreateService();
    }

    [Fact]
    public async Task ListAsync_ShouldReturnPagedOrders_WhenRepositoryReturnsData()
    {
        // Arrange
        var orders = new List<SalesOrder.Domain.Orders.Entities.Order>
        {
            OrderTestData.CreateOrder(orderNumber: "SO-1", status: "Pending"),
            OrderTestData.CreateOrder(orderNumber: "SO-2", status: "Confirmed")
        };

        _fixture.SalesOrderRepositoryMock
            .Setup(x => x.ListAsync(1, 10, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((orders, 2));

        // Act
        var result = await _service.ListAsync(1, 10, null, null, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(2, result.Total);
        Assert.Equal(2, result.Items.Count);
        Assert.Contains(result.Items, x => x.OrderNumber == "SO-1");
        Assert.Contains(result.Items, x => x.OrderNumber == "SO-2");
    }

    [Fact]
    public async Task ListAsync_ShouldApplyDefaultPageAndPageSize_WhenInvalidValuesAreProvided()
    {
        // Arrange
        _fixture.SalesOrderRepositoryMock
            .Setup(x => x.ListAsync(1, 10, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<SalesOrder.Domain.Orders.Entities.Order>(), 0));

        // Act
        var result = await _service.ListAsync(0, 0, null, null, CancellationToken.None);

        // Assert
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.Total);
    }

    [Fact]
    public async Task ListAsync_ShouldReturnEmptyList_WhenRepositoryReturnsNoData()
    {
        // Arrange
        _fixture.SalesOrderRepositoryMock
            .Setup(x => x.ListAsync(1, 10, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<SalesOrder.Domain.Orders.Entities.Order>(), 0));

        // Act
        var result = await _service.ListAsync(1, 10, null, null, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.Total);
    }
}
