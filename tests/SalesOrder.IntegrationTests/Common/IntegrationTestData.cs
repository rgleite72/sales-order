using SalesOrder.Application.DTOs.Integration;
using SalesOrder.Application.DTOs.Order;
using SalesOrder.Domain.Orders.Entities;

namespace SalesOrder.IntegrationTests.Common;

public static class IntegrationTestData
{
    public static ProductCatalogProductDto CreateProduct(
        Guid? id = null,
        string name = "Notebook Dell",
        string sku = "NOTE-001",
        decimal initialPrice = 3500m,
        string currency = "BRL")
    {
        return new ProductCatalogProductDto
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            Sku = sku,
            InitialPrice = initialPrice,
            Currency = currency
        };
    }

    public static CreateSalesOrderRequestDto CreateOrderRequest(
        Guid customerId,
        Guid productId,
        int quantity = 2,
        string currency = "BRL",
        string? notes = "Pedido de teste")
    {
        return new CreateSalesOrderRequestDto
        {
            CustomerId = customerId,
            Currency = currency,
            Notes = notes,
            Items = new List<CreateSalesOrderItemRequestDto>
            {
                new()
                {
                    ProductId = productId,
                    Quantity = quantity
                }
            }
        };
    }

    public static Order CreateOrderEntity(
        Guid? orderId = null,
        Guid? customerId = null,
        string status = "Pending",
        string currency = "BRL",
        string orderNumber = "SO-TEST-001",
        decimal unitPrice = 100m,
        int quantity = 2)
    {
        var now = DateTime.UtcNow;
        var finalOrderId = orderId ?? Guid.NewGuid();
        var finalCustomerId = customerId ?? Guid.NewGuid();

        var item = new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = finalOrderId,
            ProductId = Guid.NewGuid(),
            ProductName = "Produto Teste",
            Quantity = quantity,
            UnitPrice = unitPrice,
            TotalPrice = unitPrice * quantity,
            CreatedAt = now
        };

        return new Order
        {
            Id = finalOrderId,
            OrderNumber = orderNumber,
            CustomerId = finalCustomerId,
            OrderDate = now,
            Currency = currency,
            Status = status,
            Notes = "Seed de teste",
            TotalAmount = item.TotalPrice,
            CreatedAt = now,
            UpdatedAt = now,
            OrderItems = new List<OrderItem> { item }
        };
    }
}
