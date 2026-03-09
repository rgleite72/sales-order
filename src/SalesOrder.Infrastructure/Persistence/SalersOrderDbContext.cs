using Microsoft.EntityFrameworkCore;
using SalesOrder.Domain.Orders.Entities;


namespace SalesOrder.Infrastructure.Persistence;

public class SalesOrderDbContext : DbContext
{
    public SalesOrderDbContext(DbContextOptions<SalesOrderDbContext> options) : base(options)
    {
    }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderDocument> OrderDocuments => Set<OrderDocument>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SalesOrderDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
