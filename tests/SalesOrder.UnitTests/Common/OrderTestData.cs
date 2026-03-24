using SalesOrder.Domain.Orders.Entities;

namespace SalesOrder.UnitTests.Common;

public static class OrderTestData
{
    public static Order CreateOrder(
        Guid? id = null,
        string status = "Pending",
        string orderNumber = "SO-20260101010101",
        string currency = "BRL",
        decimal totalAmount = 100m)
    {
        var orderId = id ?? Guid.NewGuid();
        var now = DateTime.UtcNow;

        return new Order
        {
            Id = orderId,
            OrderNumber = orderNumber,
            CustomerId = Guid.NewGuid(),
            OrderDate = now,
            Currency = currency,
            Notes = "Pedido de teste",
            Status = status,
            TotalAmount = totalAmount,
            CreatedAt = now,
            UpdatedAt = now,
            OrderItems = new List<OrderItem>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    ProductId = Guid.NewGuid(),
                    ProductName = "Produto Teste",
                    Quantity = 2,
                    UnitPrice = 50m,
                    TotalPrice = 100m,
                    CreatedAt = now
                }
            }
        };
    }
}
