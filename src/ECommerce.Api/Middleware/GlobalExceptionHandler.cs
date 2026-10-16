using ECommerce.Api.Filters;
using ECommerce.Application.Common.Exceptions;
using ECommerce.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Middleware;

public class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        if (exception is ValidationException validation)
            return await WriteValidationProblemAsync(context, validation);

        var (status, title, detail) = exception switch
        {
            NotFoundException e => (StatusCodes.Status404NotFound, "Not Found", e.Message),
            ConflictException e => (StatusCodes.Status409Conflict, "Conflict", e.Message),
            BadRequestException e => (StatusCodes.Status400BadRequest, "Bad Request", e.Message),
            UnauthorizedException e => (StatusCodes.Status401Unauthorized, "Unauthorized", e.Message),
            ForbiddenException e => (StatusCodes.Status403Forbidden, "Forbidden", e.Message),
            DomainException e => (StatusCodes.Status400BadRequest, "Business rule violation", e.Message),
            BadHttpRequestException e => (e.StatusCode, "Bad Request", e.Message), // e.g. 413 body too large
            DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "Concurrency conflict",
                "The data was modified by another request. Please retry."),
            DbUpdateException => (StatusCodes.Status409Conflict, "Database conflict",
                "The operation conflicts with existing data (e.g. the record is referenced elsewhere)."),
            _ => (StatusCodes.Status500InternalServerError, "Server error", "An unexpected error occurred.")
        };

        if (status == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Unhandled exception");
        else
            logger.LogWarning(exception, "Handled exception: {Title}", title);

        context.Response.StatusCode = status;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            Exception = exception,
            ProblemDetails = new ProblemDetails { Status = status, Title = title, Detail = detail }
        });
    }

    // Same shape as the ValidationFilter response, for validators invoked inside services
    private async ValueTask<bool> WriteValidationProblemAsync(HttpContext context, ValidationException exception)
    {
        var errors = exception.Errors
            .GroupBy(e => ValidationFilter.ToCamelCasePath(e.PropertyName))
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).Distinct().ToArray());

        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            Exception = exception,
            ProblemDetails = new HttpValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more validation errors occurred."
            }
        });
    }
}
