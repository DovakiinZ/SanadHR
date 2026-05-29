using System.Text.Json;
using HR.Application.Common.Exceptions;
using HR.Application.Common.Models;

namespace HR.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, response) = exception switch
        {
            ValidationException validationEx => (StatusCodes.Status400BadRequest,
                ApiResponse.Fail("Validation failed", validationEx.Errors.SelectMany(e => e.Value).ToList())),
            NotFoundException => (StatusCodes.Status404NotFound,
                ApiResponse.Fail(exception.Message)),
            ForbiddenException => (StatusCodes.Status403Forbidden,
                ApiResponse.Fail(exception.Message)),
            ConflictException => (StatusCodes.Status409Conflict,
                ApiResponse.Fail(exception.Message)),
            _ => (StatusCodes.Status500InternalServerError,
                ApiResponse.Fail("An unexpected error occurred"))
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Unhandled exception");

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        await context.Response.WriteAsync(json);
    }
}
