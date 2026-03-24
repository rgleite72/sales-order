using Microsoft.AspNetCore.Mvc;
using SalesOrder.Application.Contracts.Documents;
using SalesOrder.Application.DTOs.Documents;

namespace SalesOrder.Api.Controllers;

[ApiController]
[Route("api/pdf-test")]
public class PdfTestController : ControllerBase
{
    private readonly IOrderPdfGenerator _orderPdfGenerator;

    public PdfTestController(IOrderPdfGenerator orderPdfGenerator)
    {
        _orderPdfGenerator = orderPdfGenerator;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] GenerateOrderPdfRequestDto request, CancellationToken ct)
    {
        var result = await _orderPdfGenerator.GenerateAsync(request, ct);

        return Ok(result);
    }
}
