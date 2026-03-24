using SalesOrder.Application.DTOs.Storage;

namespace SalesOrder.Application.Contracts.Storage;

public interface IOrderDocumentStorageService
{
    Task<UploadOrderPdfResultDto> UploadPdfAsync(
        UploadOrderPdfRequestDto request,
        CancellationToken ct);
}
