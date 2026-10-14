using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace HidroSense.Api.Middleware;

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
            await WriteProblemAsync(context, ex);
        }
    }

    private async Task WriteProblemAsync(HttpContext context, Exception ex)
    {
        var (status, title, detail) = ex switch
        {
            ValidationException v => (StatusCodes.Status400BadRequest, "Validation failed",
                string.Join("; ", v.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"))),
            NotFoundException nf => (StatusCodes.Status404NotFound, "Resource not found", nf.Message),
            DomainException de => (de.StatusCode, "Business rule violation", de.Message),
            _ => (StatusCodes.Status500InternalServerError, "Unexpected error",
                "An unexpected error occurred while processing the request.")
        };

        if (status == StatusCodes.Status500InternalServerError)
            _logger.LogError(ex, "Unhandled exception");

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}
