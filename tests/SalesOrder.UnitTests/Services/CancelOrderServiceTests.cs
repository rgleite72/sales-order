using SalesOrder.Application.Exceptions;
using SalesOrder.Application.Services.Orders;
using SalesOrder.UnitTests.Common;
using Moq;

namespace SalesOrder.UnitTests.Services;

public sealed class CancelOrderServiceTests
{
    private readonly OrderServiceTestFixture _fixture;
    private readonly OrderService _service;

    public CancelOrderServiceTests()
    {
        _fixture = new OrderServiceTestFixture();
        _service = _fixture.CreateService();
    }

    [Fact]
    public async Task CancelAsync_ShouldCancelOrder_WhenOrderIsPending()
    {
        // Arrange
        var order = OrderTestData.CreateOrder(status: "Pending");
        var oldUpdatedAt = order.UpdatedAt;

        _fixture.SalesOrderRepositoryMock
            .Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _fixture.UnitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.CancelAsync(order.Id, CancellationToken.None);

        // Assert
        Assert.Equal("Canceled", order.Status);
        Assert.True(order.UpdatedAt >= oldUpdatedAt);

        _fixture.UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CancelAsync_ShouldThrowNotFoundException_WhenOrderDoesNotExist()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        _fixture.SalesOrderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SalesOrder.Domain.Orders.Entities.Order?)null);

        // Act
        var act = async () => await _service.CancelAsync(orderId, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(act);

        _fixture.UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CancelAsync_ShouldThrowValidationException_WhenOrderIsAlreadyCanceled()
    {
        // Arrange
        var order = OrderTestData.CreateOrder(status: "Canceled");

        _fixture.SalesOrderRepositoryMock
            .Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var act = async () => await _service.CancelAsync(order.Id, CancellationToken.None);

        // Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(act);
        Assert.Contains(nameof(order.Status), exception.Errors.Keys);

        _fixture.UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
