using Microsoft.AspNetCore.Mvc;
using SalesOrder.Application.DTOs.Order;
using SalesOrder.Application.Contracts;

namespace SalesOrder.Api.Controllers;


[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateSalesOrderRequestDto dto,
        CancellationToken ct)
    {
        var result = await _orderService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
}

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _orderService.GetByIdAsync(id, ct);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        CancellationToken ct = default)
    {
        var result = await _orderService.ListAsync(page, pageSize, search, status, ct);
        return Ok(result);
    }


    [HttpPatch("{id:guid}/confirm")]
    public async Task<IActionResult> Confirm(Guid id, CancellationToken ct)
    {
        await _orderService.ConfirmAsync(id, ct);
        return NoContent();
    }    

    [HttpPatch("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        await _orderService.CancelAsync(id, ct);
        return NoContent();
    }


}
