using System.Net;

namespace SalesOrder.Application.Exceptions;

public abstract class AppException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public string Code { get; }

    protected AppException(string code, string message, HttpStatusCode statusCode)
        : base(message)
    {
        Code = code;
        StatusCode = statusCode;
    }
}
