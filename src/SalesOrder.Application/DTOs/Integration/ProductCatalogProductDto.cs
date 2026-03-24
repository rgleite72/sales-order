namespace SalesOrder.Application.DTOs.Integration;

public class ProductCatalogProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal InitialPrice { get; set; }
    public string Currency { get; set; } = string.Empty;
}
