
using Scalar.AspNetCore;
using Startup_Project.Application;
using Startup_Project.Persistance;
using Startup_Project.WebAPI.Middlewares;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApplicationServices();
builder.Services.AddPersistanceServices(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddOpenApi();
var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthorization();
app.MapControllers();
app.Run();