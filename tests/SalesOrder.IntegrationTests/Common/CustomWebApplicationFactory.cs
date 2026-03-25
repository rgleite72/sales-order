using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SalesOrder.Application.Contracts.Integration;
using SalesOrder.Domain.Orders.Entities;
using SalesOrder.Infrastructure.Persistence;



namespace SalesOrder.IntegrationTests.Common;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public FakeProductCatalogGateway FakeProductCatalogGateway { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<SalesOrderDbContext>));
            services.RemoveAll(typeof(SalesOrderDbContext));
            services.RemoveAll(typeof(IProductCatalogGateway));
            services.RemoveAll(typeof(DbConnection));

            services.AddSingleton<DbConnection>(_ =>
            {
                var connection = new SqliteConnection("DataSource=:memory:");
                connection.Open();
                return connection;
            });

            services.AddDbContext<SalesOrderDbContext>((sp, options) =>
            {
                var connection = sp.GetRequiredService<DbConnection>();
                options.UseSqlite(connection);
            });

            services.AddSingleton<IProductCatalogGateway>(FakeProductCatalogGateway);

            var serviceProvider = services.BuildServiceProvider();

            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SalesOrderDbContext>();

            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        });
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SalesOrderDbContext>();

        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();

        FakeProductCatalogGateway.Clear();
    }

    public async Task SeedOrderAsync(Order order)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SalesOrderDbContext>();

        db.Orders.Add(order);
        await db.SaveChangesAsync();
    }

    public async Task<Order?> GetOrderByIdAsync(Guid orderId)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SalesOrderDbContext>();

        return await db.Orders
            .Include(x => x.OrderItems)
            .FirstOrDefaultAsync(x => x.Id == orderId);
    }
}
