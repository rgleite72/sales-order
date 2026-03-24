using Microsoft.EntityFrameworkCore;
using SalesOrder.Api.Extensions;
using SalesOrder.Api.Middlewares;
using SalesOrder.Infrastructure.Persistence;
using SalesOrder.Application.Contracts.Integration;
using SalesOrder.Infrastructure.Integrations.ProductCatalog;
using SalesOrder.Application.Contracts;
using SalesOrder.Application.Services.Orders;
using SalesOrder.Application.Contracts.Persistence;
using SalesOrder.Application.Contracts.Documents;
using SalesOrder.Infrastructure.Documents;
using Azure.Storage.Blobs;
using SalesOrder.Application.Contracts.Storage;
using SalesOrder.Infrastructure.Storage;


var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(options =>
{
    options.IncludeScopes = true;
});

// MVC + comportamento custom
builder.Services.AddControllers();
builder.Services.AddCustomApiBehavior();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "SalesOrder API",
        Version = "v1",
        Description = "API for sales order management"
    });
});

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não foi configurada.");

builder.Services.AddDbContext<SalesOrderDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString);


builder.Services.AddSingleton(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("AzureBlobStorage")
        ?? throw new InvalidOperationException("Connection string 'AzureBlobStorage' não foi configurada.");

    return new BlobServiceClient(connectionString);
});


builder.Services.AddHttpClient<IProductCatalogGateway, ProductCatalogGateway>((sp, client) =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var baseUrl = configuration["IntegrationSettings:ProductCatalogBaseUrl"];

    client.BaseAddress = new Uri(baseUrl!);
});


builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ISalesOrderRepository, SalesOrderRepository>();
builder.Services.AddScoped<IUnitOfWork, EfUnitOfWork>();

//Gerador do PDF
builder.Services.AddScoped<IOrderPdfGenerator, OrderPdfGenerator>();
//Envia arquivo para Blob
builder.Services.AddScoped<IOrderDocumentStorageService, OrderDocumentStorageService>();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "SalesOrder API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseMiddleware<RequestCorrelationMiddleware>();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.Use(async (ctx, next) =>
{
    var logger = ctx.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("Request");
    var traceId = RequestCorrelationMiddleware.GetRequestId(ctx);

    logger.LogInformation("HTTP {Method} {Path} TraceId={TraceId}",
        ctx.Request.Method, ctx.Request.Path, traceId);

    try
    {
        await next();
    }
    finally
    {
        logger.LogInformation("HTTP {StatusCode} {Method} {Path} TraceId={TraceId}",
            ctx.Response.StatusCode, ctx.Request.Method, ctx.Request.Path, traceId);
    }
});

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
