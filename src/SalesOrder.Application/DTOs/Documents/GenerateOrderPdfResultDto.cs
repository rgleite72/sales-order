namespace SalesOrder.Application.DTOs.Documents;

public class GenerateOrderPdfResultDto
{
    public string FileName { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public string ContentType { get; set; } = "application/pdf";
}
