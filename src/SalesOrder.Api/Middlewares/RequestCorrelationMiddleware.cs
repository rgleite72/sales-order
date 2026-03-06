namespace SalesOrder.Api.Middlewares;

public sealed class RequestCorrelationMiddleware
{
    private const string HeaderName = "X-Request-Id";
    private readonly RequestDelegate _next;

    public RequestCorrelationMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext context)
    {
        var requestId = context.Request.Headers.TryGetValue(HeaderName, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value.ToString()
            : Guid.NewGuid().ToString("N");

        context.Items[HeaderName] = requestId;
        context.Response.Headers[HeaderName] = requestId;

        await _next(context);
    }

    public static string GetRequestId(HttpContext context)
        => context.Items.TryGetValue(HeaderName, out var v) ? v?.ToString() ?? context.TraceIdentifier : context.TraceIdentifier;
}
