using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace StudentPortal.Shared;

// Week 9's Task 9.2 handler, shared so every service fails the same way:
// the exception is logged with a trace id, the caller gets a ProblemDetails
// 500 with no internals.
public class ApiExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ApiExceptionHandler> logger;
    private readonly IProblemDetailsService problemDetails;

    public ApiExceptionHandler(ILogger<ApiExceptionHandler> logger, IProblemDetailsService problemDetails)
    {
        this.logger = logger;
        this.problemDetails = problemDetails;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception for {Method} {Path} (trace {TraceId}).",
            httpContext.Request.Method, httpContext.Request.Path, httpContext.TraceIdentifier);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred.",
                Detail = "The request could not be completed. Please try again later."
            }
        });
    }
}
