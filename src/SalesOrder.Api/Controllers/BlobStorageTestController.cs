using Microsoft.AspNetCore.Mvc;
using SalesOrder.Application.Contracts.Storage;
using SalesOrder.Application.DTOs.Storage;

namespace SalesOrder.Api.Controllers;

[ApiController]
[Route("api/blob-storage-test")]
public class BlobStorageTestController : ControllerBase
{
    private readonly IOrderDocumentStorageService _orderDocumentStorageService;

    public BlobStorageTestController(IOrderDocumentStorageService orderDocumentStorageService)
    {
        _orderDocumentStorageService = orderDocumentStorageService;
    }

    [HttpPost("upload-local-pdf")]
    public async Task<IActionResult> UploadLocalPdf([FromBody] UploadOrderPdfRequestDto request, CancellationToken ct)
    {
        var result = await _orderDocumentStorageService.UploadPdfAsync(request, ct);

        return Ok(result);
    }
}
