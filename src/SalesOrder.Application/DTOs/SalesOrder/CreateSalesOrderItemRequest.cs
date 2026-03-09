namespace SalesOrder.Application.DTOs.Order;

public class CreateSalesOrderItemRequestDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
