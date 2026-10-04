using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace StudentApi;

// Task 9.2 - the last line of defence. Anything a controller or service
// didn't turn into a deliberate status code (a database outage, a bug)
// arrives here. It's logged in full on the server, and the client gets a
// plain RFC 7807 ProblemDetails 500 with a trace id it can quote. The client
// never sees a stack trace, a connection string or a SQL error.
// The React client turns this into "The server hit a problem. Please try
// again." instead of crashing.
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
