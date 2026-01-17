using Startup_Project.Application.Models;
using System.Text.Json;

namespace Startup_Project.WebAPI.Middlewares;

public class ExceptionHandlingMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            ResponseModel<bool> responseModel = ResponseModel<bool>.FailureResponse(ex.Message);
            {
                context.Response.ContentType = "application/json";
                var serializedResponse = JsonSerializer.Serialize(responseModel);
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync(serializedResponse);
            }
            ;
        }
    }

}
