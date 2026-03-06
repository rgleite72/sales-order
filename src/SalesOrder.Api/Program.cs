using Microsoft.EntityFrameworkCore;
using SalesOrder.Api.Extensions;
using SalesOrder.Api.Middlewares;
using SalesOrder.Infrastructure.Persistence;

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
