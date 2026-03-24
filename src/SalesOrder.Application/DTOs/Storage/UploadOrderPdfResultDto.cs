namespace SalesOrder.Application.DTOs.Storage;

public class UploadOrderPdfResultDto
{
    public string BlobName { get; set; } = string.Empty;

    public string BlobUrl { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;
}
