using System.Text.Json;
using HR.Application.Common.Exceptions;
using HR.Application.Common.Models;
using HR.Domain.Engines.Finance.StateMachine;

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
            // Illegal lifecycle moves (run / transaction state machines) are a conflict, not a crash.
            InvalidStateTransitionException => (StatusCodes.Status409Conflict,
                ApiResponse.Fail(exception.Message)),
            InvalidPayrollTransactionStateException => (StatusCodes.Status409Conflict,
                ApiResponse.Fail(exception.Message)),
            // Explicit business-rule violations carry a user-facing reason.
            DomainException => (StatusCodes.Status422UnprocessableEntity,
                ApiResponse.Fail(exception.Message)),
            // Safety net: the engine/service layer signals business rules (inactive type,
            // amount<0, duplicate code, …) via InvalidOperationException. Surface the real
            // reason as 422 instead of an opaque 500. Logged as a warning, not swallowed.
            InvalidOperationException => (StatusCodes.Status422UnprocessableEntity,
                ApiResponse.Fail(exception.Message)),
            _ => (StatusCodes.Status500InternalServerError,
                ApiResponse.Fail("An unexpected error occurred"))
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Unhandled exception");
        else if (statusCode == StatusCodes.Status422UnprocessableEntity && exception is InvalidOperationException)
            _logger.LogWarning(exception, "Business-rule violation surfaced as 422 (consider migrating to DomainException)");

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        await context.Response.WriteAsync(json);
    }
}
