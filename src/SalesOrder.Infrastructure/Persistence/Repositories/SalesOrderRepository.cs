using Microsoft.EntityFrameworkCore;
using SalesOrder.Domain.Orders.Entities;
using SalesOrder.Application.Contracts.Persistence;

namespace SalesOrder.Infrastructure.Persistence;

public class SalesOrderRepository : ISalesOrderRepository
{
    private readonly SalesOrderDbContext _db;

    public SalesOrderRepository(SalesOrderDbContext db)
    {
        _db = db;
    }

    public Task AddAsync(Order order, CancellationToken ct = default) =>
        _db.Orders.AddAsync(order, ct).AsTask();

    public Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct) =>
        _db.Orders.AnyAsync(x => x.Id == id, ct);

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken ct) =>
        _db.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<Order?> GetByIdWithItemsAsync(Guid id, CancellationToken ct)
    {
        return _db.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task UpdateStatusAsync(Guid id, string status, CancellationToken ct)
    {
        var order = await _db.Orders
            .FirstOrDefaultAsync(o => o.Id == id, ct);

        if (order is null)
        {
            return;
        }

        order.Status = status;
        order.UpdatedAt = DateTime.UtcNow;
    }

    public async Task<(List<Order> Items, int Total)> ListAsync(
        int page,
        int pageSize,
        string? search,
        string? status,
        CancellationToken ct)
    {
        var query = _db.Orders
            .Include(o => o.OrderItems)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(o => o.OrderNumber.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(o => o.Status == status);
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(o => o.OrderDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }
}
