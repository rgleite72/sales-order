namespace SalesOrder.Domain.Orders.Entities;

public class OrderDocument
{
    
    public Guid Id {get; set;}
    public Guid OrderId {get; set;}
    public string FileName {get; set;} = string.Empty;

    public string BlobUrl {get; set;} = string.Empty;

    public string ContentType {get; set;} = string.Empty;

    public DateTime UploadedAt {get; set;}

    public Order Order { get; set; } = null!;

}
