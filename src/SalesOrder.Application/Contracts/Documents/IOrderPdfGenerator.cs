using SalesOrder.Application.DTOs.Documents;

namespace SalesOrder.Application.Contracts.Documents;

public interface IOrderPdfGenerator
{
    Task<GenerateOrderPdfResultDto> GenerateAsync(
        GenerateOrderPdfRequestDto request,
        CancellationToken ct);
}
