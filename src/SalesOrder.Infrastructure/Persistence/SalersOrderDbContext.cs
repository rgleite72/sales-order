using Microsoft.EntityFrameworkCore;

namespace SalesOrder.Infrastructure.Persistence;

public class SalesOrderDbContext: DbContext
{
    public SalesOrderDbContext(DbContextOptions<SalesOrderDbContext> options) : base(options){}


    // public DbSet<Product> Products => Set<Product>();

}
