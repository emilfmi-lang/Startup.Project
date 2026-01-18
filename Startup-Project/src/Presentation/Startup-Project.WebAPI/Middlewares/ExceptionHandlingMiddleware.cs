using Startup_Project.Application.Models;
using System.Text.Json;

namespace Startup_Project.WebAPI.Middlewares;

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
            _logger.LogError(ex, "Sistemdə xəta baş verdi: {Message}", ex.Message);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            var responseModel = ResponseModel<bool>.FailureResponse("Gözlənilməz bir xəta baş verdi.");


            var serializedResponse = JsonSerializer.Serialize(responseModel);
            await context.Response.WriteAsync(serializedResponse);
        }
    }

}
