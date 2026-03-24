using Moq;
using SalesOrder.Application.DTOs.Integration;
using SalesOrder.Application.DTOs.Order;
using SalesOrder.Application.Exceptions;
using SalesOrder.Application.Services.Orders;
using SalesOrder.Domain.Orders.Entities;
using SalesOrder.UnitTests.Common;

namespace SalesOrder.UnitTests.Services;

public sealed class CreateOrderServiceTests
{
    private readonly OrderServiceTestFixture _fixture;
    private readonly OrderService _service;

    public CreateOrderServiceTests()
    {
        _fixture = new OrderServiceTestFixture();
        _service = _fixture.CreateService();
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateOrder_WhenRequestIsValid()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var request = new CreateSalesOrderRequestDto
        {
            CustomerId = customerId,
            Currency = "brl",
            Notes = "Pedido teste",
            Items = new List<CreateSalesOrderItemRequestDto>
            {
                new()
                {
                    ProductId = productId,
                    Quantity = 2
                }
            }
        };

        _fixture.UnitOfWorkMock
            .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_fixture.UnitOfWorkTransactionMock.Object);

        _fixture.ProductCatalogGatewayMock
            .Setup(x => x.GetProductByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProductCatalogProductDto
            {
                Id = productId,
                Name = "Notebook",
                Sku = "NOTE-001",
                Currency = "BRL",
                InitialPrice = 1500m
            });

        Order? capturedOrder = null;

        _fixture.SalesOrderRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Callback<Order, CancellationToken>((order, _) => capturedOrder = order)
            .Returns(Task.CompletedTask);

        _fixture.UnitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _fixture.UnitOfWorkTransactionMock
            .Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(customerId, result.CustomerId);
        Assert.Equal("BRL", result.Currency);
        Assert.Equal("Pending", result.Status);
        Assert.Equal(3000m, result.TotalAmount);
        Assert.StartsWith("SO-", result.OrderNumber);

        Assert.NotNull(capturedOrder);
        Assert.Equal("Pending", capturedOrder!.Status);
        Assert.Equal(3000m, capturedOrder.TotalAmount);
        Assert.Single(capturedOrder.OrderItems);

        _fixture.SalesOrderRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _fixture.UnitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        _fixture.UnitOfWorkTransactionMock.Verify(
            x => x.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        _fixture.UnitOfWorkTransactionMock.Verify(
            x => x.RollbackAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowValidationException_WhenCustomerIdIsEmpty()
    {
        // Arrange
        var request = new CreateSalesOrderRequestDto
        {
            CustomerId = Guid.Empty,
            Currency = "BRL",
            Items = new List<CreateSalesOrderItemRequestDto>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    Quantity = 1
                }
            }
        };

        // Act
        var act = async () => await _service.CreateAsync(request, CancellationToken.None);

        // Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(act);
        Assert.Equal("VALIDATION_ERROR", exception.Code);
        Assert.Contains(nameof(request.CustomerId), exception.Errors.Keys);

        _fixture.SalesOrderRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowValidationException_WhenItemsAreEmpty()
    {
        // Arrange
        var request = new CreateSalesOrderRequestDto
        {
            CustomerId = Guid.NewGuid(),
            Currency = "BRL",
            Items = new List<CreateSalesOrderItemRequestDto>()
        };

        // Act
        var act = async () => await _service.CreateAsync(request, CancellationToken.None);

        // Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(act);
        Assert.Contains(nameof(request.Items), exception.Errors.Keys);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowValidationException_WhenAnyItemQuantityIsLessThanOrEqualToZero()
    {
        // Arrange
        var request = new CreateSalesOrderRequestDto
        {
            CustomerId = Guid.NewGuid(),
            Currency = "BRL",
            Items = new List<CreateSalesOrderItemRequestDto>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    Quantity = 0
                }
            }
        };

        // Act
        var act = async () => await _service.CreateAsync(request, CancellationToken.None);

        // Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(act);
        Assert.Contains(nameof(request.Items), exception.Errors.Keys);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowValidationException_WhenCurrencyIsEmpty()
    {
        // Arrange
        var request = new CreateSalesOrderRequestDto
        {
            CustomerId = Guid.NewGuid(),
            Currency = "",
            Items = new List<CreateSalesOrderItemRequestDto>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    Quantity = 1
                }
            }
        };

        // Act
        var act = async () => await _service.CreateAsync(request, CancellationToken.None);

        // Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(act);
        Assert.Contains(nameof(request.Currency), exception.Errors.Keys);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowNotFoundException_WhenProductDoesNotExist()
    {
        // Arrange
        var productId = Guid.NewGuid();

        var request = new CreateSalesOrderRequestDto
        {
            CustomerId = Guid.NewGuid(),
            Currency = "BRL",
            Items = new List<CreateSalesOrderItemRequestDto>
            {
                new()
                {
                    ProductId = productId,
                    Quantity = 1
                }
            }
        };

        _fixture.UnitOfWorkMock
            .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_fixture.UnitOfWorkTransactionMock.Object);

        _fixture.ProductCatalogGatewayMock
            .Setup(x => x.GetProductByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductCatalogProductDto?)null);

        _fixture.UnitOfWorkTransactionMock
            .Setup(x => x.RollbackAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var act = async () => await _service.CreateAsync(request, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(act);

        _fixture.UnitOfWorkTransactionMock.Verify(
            x => x.RollbackAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        _fixture.UnitOfWorkTransactionMock.Verify(
            x => x.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowValidationException_WhenProductCurrencyDoesNotMatchOrderCurrency()
    {
        // Arrange
        var productId = Guid.NewGuid();

        var request = new CreateSalesOrderRequestDto
        {
            CustomerId = Guid.NewGuid(),
            Currency = "BRL",
            Items = new List<CreateSalesOrderItemRequestDto>
            {
                new()
                {
                    ProductId = productId,
                    Quantity = 1
                }
            }
        };

        _fixture.UnitOfWorkMock
            .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_fixture.UnitOfWorkTransactionMock.Object);

        _fixture.ProductCatalogGatewayMock
            .Setup(x => x.GetProductByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProductCatalogProductDto
            {
                Id = productId,
                Name = "Mouse",
                Sku = "MOUSE-001",
                Currency = "USD",
                InitialPrice = 10m
            });

        _fixture.UnitOfWorkTransactionMock
            .Setup(x => x.RollbackAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var act = async () => await _service.CreateAsync(request, CancellationToken.None);

        // Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(act);
        Assert.Contains(nameof(request.Currency), exception.Errors.Keys);

        _fixture.UnitOfWorkTransactionMock.Verify(
            x => x.RollbackAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        _fixture.UnitOfWorkTransactionMock.Verify(
            x => x.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
