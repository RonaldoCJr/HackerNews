using HackerNews.Infrastructure.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc; 
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace HackerNews.Infrastructure.Middleware
{
    public static class ExceptionMiddleware
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
                    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

                    var problemDetails = new ProblemDetails
                    {
                        Instance = context.Request.Path,
                        Type = "https://tools.ietf.org/html/rfc7231"
                    };

                    switch (exception)
                    {
                        case BrokenCircuitException ex:
                            logger.LogError(ex, "Circuit breaker triggered. Service is temporarily unavailable.");

                            context.Response.StatusCode = StatusCodes.Status502BadGateway;
                            problemDetails.Status = StatusCodes.Status502BadGateway;
                            problemDetails.Title = "Service Temporarily Unavailable (Circuit Breaker)";
                            problemDetails.Detail = "Upstream service is temporarily unavailable. Please try again later.";
                            break;

                        case TimeoutRejectedException ex:
                            logger.LogError(ex, "Request timed out via resilience policy.");

                            context.Response.StatusCode = StatusCodes.Status504GatewayTimeout;
                            problemDetails.Status = StatusCodes.Status504GatewayTimeout;
                            problemDetails.Title = "Gateway Timeout";
                            problemDetails.Detail = "The request to the upstream service timed out. Please try again.";
                            break;

                        case ExternalServiceException ex:
                            logger.LogError(ex, "External service error. StatusCode: {StatusCode}, ErrorContent: {ErrorContent}", ex.StatusCode, ex.ErrorContent);

                            var statusCode = ex.StatusCode ?? StatusCodes.Status502BadGateway;
                            context.Response.StatusCode = statusCode;
                            problemDetails.Status = statusCode;
                            problemDetails.Title = "External Service Error";
                            problemDetails.Detail = "Error communicating with external service.";
                            break;

                        default:
                            logger.LogError(exception, "Unhandled error");

                            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                            problemDetails.Status = StatusCodes.Status500InternalServerError;
                            problemDetails.Title = "Internal Server Error";
                            problemDetails.Detail = "An unhandled error occurred internally within the server.";
                            break;
                    }

                    // 3. Forçamos o Content-Type correto para JSON e escrevemos o objeto estruturado
                    context.Response.ContentType = "application/problem+json";
                    await context.Response.WriteAsJsonAsync(problemDetails);
                });
            });

            return app;
        }
    }
}