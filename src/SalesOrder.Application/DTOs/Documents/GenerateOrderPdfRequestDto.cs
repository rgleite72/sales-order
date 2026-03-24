namespace SalesOrder.Application.DTOs.Documents;

public class GenerateOrderPdfRequestDto
{
    public Guid OrderId { get; set; }

    public string OrderNumber { get; set; } = string.Empty;

    public string CustomerName { get; set; } = string.Empty;

    public DateTime OrderDate { get; set; }

    public decimal TotalAmount { get; set; }

    public string Currency { get; set; } = "BRL";
}
