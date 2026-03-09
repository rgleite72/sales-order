namespace SalesOrder.Application.DTOs.Integration;

public class ProductCatalogProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = string.Empty;
}
