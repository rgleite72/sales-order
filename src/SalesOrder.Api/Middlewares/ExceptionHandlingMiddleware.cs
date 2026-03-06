using System.Net;
using System.Text.Json;
using SalesOrder.Application.Contracts;
using SalesOrder.Application.Exceptions;

namespace SalesOrder.Api.Middlewares;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await Handle(context, ex);
        }
    }

    private async Task Handle(HttpContext context, Exception ex)
    {
        var traceId = RequestCorrelationMiddleware.GetRequestId(context);

        _logger.LogError(ex,
            "Unhandled exception. TraceId={TraceId} Path={Path} Method={Method}",
            traceId, context.Request.Path, context.Request.Method);

        context.Response.ContentType = "application/json";

        if (ex is ValidationException vex)
        {
            context.Response.StatusCode = (int)vex.StatusCode;
            await Write(context, ApiResponse<object>.Fail(new ApiError
            {
                Code = vex.Code,
                Message = vex.Message,
                Details = vex.Errors
            }, traceId));
            return;
        }

        if (ex is AppException aex)
        {
            context.Response.StatusCode = (int)aex.StatusCode;
            await Write(context, ApiResponse<object>.Fail(new ApiError
            {
                Code = aex.Code,
                Message = aex.Message
            }, traceId));
            return;
        }

        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        await Write(context, ApiResponse<object>.Fail(new ApiError
        {
            Code = "INTERNAL_ERROR",
            Message = "Ocorreu um erro inesperado."
        }, traceId));
    }

    private static Task Write(HttpContext context, ApiResponse<object> payload)
    {
        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return context.Response.WriteAsync(json);
    }
}
