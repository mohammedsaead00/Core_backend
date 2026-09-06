using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CoreGym.Api;

/// <summary>
/// Maps the domain-level exceptions thrown by the application services to
/// problem-details responses; anything else falls through to the 500 path.
/// </summary>
public class CoreGymExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        int? statusCode;
        string? title;

        if (exception is KeyNotFoundException)
        {
            statusCode = StatusCodes.Status404NotFound;
            title = "Not found";
        }
        else if (exception is UnauthorizedAccessException)
        {
            statusCode = StatusCodes.Status403Forbidden;
            title = "Forbidden";
        }
        else if (exception is ArgumentException)
        {
            statusCode = StatusCodes.Status400BadRequest;
            title = "Invalid request";
        }
        else
        {
            return false;
        }

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message,
        };

        httpContext.Response.StatusCode = statusCode.Value;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
}
