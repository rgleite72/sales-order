using Microsoft.Extensions.Logging;
using Moq;
using SalesOrder.Application.Contracts.Integration;
using SalesOrder.Application.Contracts.Persistence;
using SalesOrder.Application.Services.Orders;

namespace SalesOrder.UnitTests.Common;

public sealed class OrderServiceTestFixture
{
    public Mock<IUnitOfWork> UnitOfWorkMock { get; } = new();
    public Mock<IUnitOfWorkTransaction> UnitOfWorkTransactionMock { get; } = new();
    public Mock<ISalesOrderRepository> SalesOrderRepositoryMock { get; } = new();
    public Mock<IProductCatalogGateway> ProductCatalogGatewayMock { get; } = new();
    public Mock<ILogger<OrderService>> LoggerMock { get; } = new();

    public OrderService CreateService()
    {
        return new OrderService(
            UnitOfWorkMock.Object,
            SalesOrderRepositoryMock.Object,
            ProductCatalogGatewayMock.Object,
            LoggerMock.Object
        );
    }
}
