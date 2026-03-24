namespace SalesOrder.Application.DTOs.Storage;

public class UploadOrderPdfRequestDto
{
    public string FilePath { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = "application/pdf";
}
