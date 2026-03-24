using SalesOrder.Application.Exceptions;
using SalesOrder.Application.Services.Orders;
using SalesOrder.UnitTests.Common;
using Moq;

namespace SalesOrder.UnitTests.Services;

public sealed class GetOrderByIdServiceTests
{
    private readonly OrderServiceTestFixture _fixture;
    private readonly OrderService _service;

    public GetOrderByIdServiceTests()
    {
        _fixture = new OrderServiceTestFixture();
        _service = _fixture.CreateService();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnOrder_WhenOrderExists()
    {
        // Arrange
        var order = OrderTestData.CreateOrder();

        _fixture.SalesOrderRepositoryMock
            .Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var result = await _service.GetByIdAsync(order.Id, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(order.Id, result.Id);
        Assert.Equal(order.OrderNumber, result.OrderNumber);
        Assert.Equal(order.CustomerId, result.CustomerId);
        Assert.Equal(order.Status, result.Status);
        Assert.Equal(order.Currency, result.Currency);
        Assert.Equal(order.TotalAmount, result.TotalAmount);
        Assert.NotNull(result.Items);
        Assert.Single(result.Items);

        var item = result.Items.First();
        var sourceItem = order.OrderItems.First();

        Assert.Equal(sourceItem.ProductId, item.ProductId);
        Assert.Equal(sourceItem.ProductName, item.ProductName);
        Assert.Equal(sourceItem.Quantity, item.Quantity);
        Assert.Equal(sourceItem.UnitPrice, item.UnitPrice);
        Assert.Equal(sourceItem.TotalPrice, item.TotalPrice);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrowNotFoundException_WhenOrderDoesNotExist()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        _fixture.SalesOrderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SalesOrder.Domain.Orders.Entities.Order?)null);

        // Act
        var act = async () => await _service.GetByIdAsync(orderId, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(act);
    }
}
