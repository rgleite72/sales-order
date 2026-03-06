using System.Net;

namespace SalesOrder.Application.Exceptions;

public sealed class ValidationException : AppException
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationException(string message, IReadOnlyDictionary<string, string[]> errors, string code = "VALIDATION_ERROR")
        : base(code, message, HttpStatusCode.UnprocessableEntity)
    {
        Errors = errors;
    }
}
