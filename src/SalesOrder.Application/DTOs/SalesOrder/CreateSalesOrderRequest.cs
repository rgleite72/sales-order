namespace SalesOrder.Application.DTOs.Order;

public class CreateSalesOrderRequestDto
{
    

    public string OrderNumber {get; set;} = string.Empty;

    public Guid CustomerId {get; set;}

    public DateTime OrderDate {get; set;}

    public string Currency {get; set;} = "BRL";

    public string? Notes {get; set;}

    public List<CreateSalesOrderItemRequestDto> Items { get; set; } = new();


}
