using Microsoft.AspNetCore.Mvc;
using SalesOrder.Application.Contracts;
using SalesOrder.Api.Middlewares;

namespace SalesOrder.Api.Controllers;

[ApiController]
public abstract class BaseApiController : ControllerBase
{
    protected string TraceId => RequestCorrelationMiddleware.GetRequestId(HttpContext);

    protected ActionResult<ApiResponse<T>> OkResponse<T>(T data)
        => Ok(ApiResponse<T>.Ok(data, TraceId));
}




