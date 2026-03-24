using System.Net;

namespace SalesOrder.Application.Exceptions;

public sealed class ValidationException : AppException
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationException(
        string message,
        IReadOnlyDictionary<string, string[]> errors,
        string code = "VALIDATION_ERROR")
        : base(code, message, HttpStatusCode.UnprocessableEntity)
    {
        Errors = errors;
    }

    // 🔹 Atalho para erro em um único campo
    public static ValidationException ForField(string field, string message)
    {
        return new ValidationException(
            "Validation failed",
            new Dictionary<string, string[]>
            {
                { field, new[] { message } }
            });
    }

    // 🔹 Atalho para campo obrigatório (caso mais comum)
    public static ValidationException Required(string field)
    {
        return ForField(field, $"{field} is required.");
    }
}
