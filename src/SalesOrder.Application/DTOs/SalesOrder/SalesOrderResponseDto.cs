namespace SalesOrder.Application.DTOs.Order;

public class SalesOrderResponseDto
{

    public Guid Id {get; set;}
    public string OrderNumber {get; set;} = string.Empty;

    public Guid CustomerId {get; set;}

    public DateTime OrderDate {get; set;}

    public decimal TotalAmount {get; set;}

    public string Status {get; set;} = string.Empty;

    public string Currency {get; set;} = string.Empty;

    public string? Notes {get; set;}

    public DateTime CreatedAt{get; set;}
    public DateTime UpdatedAt{get; set;}

    public List<SalesOrderItemResponseDto> Items { get; set; } = new();

}
