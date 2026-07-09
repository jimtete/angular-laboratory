using LearningLab.Data.Models.DTOs;
using Microsoft.AspNetCore.Diagnostics;

namespace LearningLab.ErrorHandling;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "An unhandled exception occurred.");

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(new ApiResponse<object>
        {
            StatusCode = StatusCodes.Status500InternalServerError,
            Message = "An unexpected error occurred.",
            Data = null
        }, cancellationToken);

        return true;
    }
}
