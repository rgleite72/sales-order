using Microsoft.EntityFrameworkCore.Storage;
using SalesOrder.Application.Contracts.Persistence;

namespace SalesOrder.Infrastructure.Persistence;

public sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly SalesOrderDbContext _db;

    public EfUnitOfWork(SalesOrderDbContext db)
    {
        _db = db;
    }

    public async Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken ct)
    {
        var tx = await _db.Database.BeginTransactionAsync(ct);
        return new EfUnitOfWorkTransaction(tx);
    }

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);

    private sealed class EfUnitOfWorkTransaction : IUnitOfWorkTransaction
    {
        private readonly IDbContextTransaction _tx;

        public EfUnitOfWorkTransaction(IDbContextTransaction tx)
        {
            _tx = tx;
        }

        public Task CommitAsync(CancellationToken ct) => _tx.CommitAsync(ct);

        public Task RollbackAsync(CancellationToken ct) => _tx.RollbackAsync(ct);

        public ValueTask DisposeAsync() => _tx.DisposeAsync();
    }
}
