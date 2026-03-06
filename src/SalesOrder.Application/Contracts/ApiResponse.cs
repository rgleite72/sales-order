namespace SalesOrder.Application.Contracts;

public sealed class ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public ApiError? Error { get; init; }
    public string TraceId { get; init; } = default!;

    public static ApiResponse<T> Ok(T data, string traceId)
        => new() { Success = true, Data = data, TraceId = traceId };

    public static ApiResponse<T> Fail(ApiError error, string traceId)
        => new() { Success = false, Error = error, TraceId = traceId };
}

public sealed class ApiError
{
    public string Code { get; init; } = default!;
    public string Message { get; init; } = default!;
    public IReadOnlyDictionary<string, string[]>? Details { get; init; }
}
