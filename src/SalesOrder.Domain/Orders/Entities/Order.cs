
namespace SalesOrder.Domain.Orders.Entities;

public class Order {
    
    public Guid Id {get; set;}
    public string OrderNumber {get; set;} = string.Empty;

    public Guid CustomerId {get; set;}

    public DateTime OrderDate {get; set;}

    public decimal TotalAmount {get; set;}

    public string Status {get; set;} = "Draft";

    public string Currency {get; set;} = "BRL";

    public string? Notes {get; set;}

    public DateTime CreatedAt{get; set;}
    public DateTime UpdatedAt{get; set;}

    public ICollection<OrderItem> OrderItems {get; set;} = new List<OrderItem>();

    public OrderDocument? Document { get; set; }

}
