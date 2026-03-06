using Microsoft.AspNetCore.Mvc;
using SalesOrder.Application.Contracts;
using SalesOrder.Api.Middlewares;

namespace SalesOrder.Api.Extensions;

public static class ApiBehaviorExtensions
{
    public static IServiceCollection AddCustomApiBehavior(this IServiceCollection services)
    {
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        k => k.Key,
                        v => v.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                var traceId = RequestCorrelationMiddleware.GetRequestId(context.HttpContext);

                var payload = ApiResponse<object>.Fail(new ApiError
                {
                    Code = "MODEL_VALIDATION_ERROR",
                    Message = "Payload inválido.",
                    Details = errors
                }, traceId);

                return new BadRequestObjectResult(payload);
            };
        });

        return services;
    }
}
