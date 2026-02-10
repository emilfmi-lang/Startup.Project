using FluentValidation;
using Startup_Project.Application.Models;
using Startup_Project.Domain.Enums.Errors;
using Startup_Project.Domain.Exceptions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Startup_Project.WebAPI.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

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
        catch (ValidationException ex)
        {
            await HandleValidationExceptionAsync(context, ex);
        }
        catch (BaseException ex)
        {
            await HandleBaseExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            await HandleUnknownExceptionAsync(context, ex);
        }
    }

    private async Task HandleBaseExceptionAsync(HttpContext context, BaseException ex)
    {
        _logger.LogWarning(ex, 
            "Business Exception: [{ErrorCode}] {Message}", 
            ex.ErrorCode, ex.Message);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = ex.StatusCode;
        context.Response.Headers.Append("X-Error-Code", ex.ErrorCode.ToString());

        var response = ErrorResponseModel.FromException(ex.ErrorCode, ex.Message);
        var json = JsonSerializer.Serialize(response, _jsonOptions);
        await context.Response.WriteAsync(json);
    }

    private async Task HandleUnknownExceptionAsync(HttpContext context, Exception ex)
    {
        _logger.LogError(ex, "Unhandled Exception: {Message}", ex.Message);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var response = ErrorResponseModel.InternalError();
        var json = JsonSerializer.Serialize(response, _jsonOptions);
        await context.Response.WriteAsync(json);
    }

    private async Task HandleValidationExceptionAsync(HttpContext context, ValidationException ex)
    {
        var errors = ex.Errors.Select(e => e.ErrorMessage).ToList();

        _logger.LogWarning("Validation Exception: {Errors}", string.Join(", ", errors));

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status400BadRequest;

        var response = ErrorResponseModel.FromException(BusinessErrorCodes.ValidationFailed, errors);
        var json = JsonSerializer.Serialize(response, _jsonOptions);
        await context.Response.WriteAsync(json);
    }
}
