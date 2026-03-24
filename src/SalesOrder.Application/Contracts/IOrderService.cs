using SalesOrder.Application.DTOs.Order;


namespace SalesOrder.Application.Contracts;

public interface IOrderService
{

    Task<SalesOrderResponseDto> CreateAsync(CreateSalesOrderRequestDto dto, CancellationToken ct);
    Task<SalesOrderResponseDto> GetByIdAsync(Guid id, CancellationToken ct);
    Task<PagedResponseDto<SalesOrderResponseDto>> ListAsync(
        int page,
        int pageSize,
        string? search,
        string? status,
        CancellationToken ct);
    Task ConfirmAsync(Guid id, CancellationToken ct);
    Task CancelAsync(Guid id, CancellationToken ct);    
    
}
