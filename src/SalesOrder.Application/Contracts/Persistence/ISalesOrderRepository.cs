using SalesOrder.Domain.Orders.Entities;

namespace SalesOrder.Application.Contracts.Persistence;

public interface ISalesOrderRepository
{
    Task AddAsync(Order order, CancellationToken ct = default);

    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct);

    Task<Order?> GetByIdWithItemsAsync(Guid id, CancellationToken ct);

    Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct);

    Task UpdateStatusAsync(Guid id, string status, CancellationToken ct);

    Task<(List<Order> Items, int Total)> ListAsync(
        int page,
        int pageSize,
        string? search,
        string? status,
        CancellationToken ct);
}
