using Microsoft.Extensions.Configuration;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using SalesOrder.Application.Contracts.Documents;
using SalesOrder.Application.DTOs.Documents;

namespace SalesOrder.Infrastructure.Documents;

public class OrderPdfGenerator : IOrderPdfGenerator
{
    private readonly string _outputFolder;

    public OrderPdfGenerator(IConfiguration configuration)
    {
        _outputFolder = configuration["PdfSettings:OutputFolder"]!;
    }

    public Task<GenerateOrderPdfResultDto> GenerateAsync(
        GenerateOrderPdfRequestDto request,
        CancellationToken ct)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        Directory.CreateDirectory(_outputFolder);

        var fileName = $"order-{request.OrderNumber}-{Guid.NewGuid():N}.pdf";
        var filePath = Path.Combine(_outputFolder, fileName);

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);

                page.Header()
                    .Text("Sales Order")
                    .SemiBold()
                    .FontSize(20);

                page.Content().Column(column =>
                {
                    column.Spacing(10);

                    column.Item().Text($"Order Id: {request.OrderId}");
                    column.Item().Text($"Order Number: {request.OrderNumber}");
                    column.Item().Text($"Customer Name: {request.CustomerName}");
                    column.Item().Text($"Order Date: {request.OrderDate:dd/MM/yyyy HH:mm}");
                    column.Item().Text($"Total Amount: {request.TotalAmount:N2} {request.Currency}");
                });

                page.Footer()
                    .AlignCenter()
                    .Text(text =>
                    {
                        text.Span("Generated on ");
                        text.Span(DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss"));
                    });
            });
        }).GeneratePdf(filePath);

        var result = new GenerateOrderPdfResultDto
        {
            FileName = fileName,
            FilePath = filePath,
            ContentType = "application/pdf"
        };

        return Task.FromResult(result);
    }
}
