
using Scalar.AspNetCore;
using Startup_Project.Application;
using Startup_Project.Persistance;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApplicationServices();
builder.Services.AddPersistanceServices(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddOpenApi();
var app = builder.Build();
app.MapOpenApi();
app.MapScalarApiReference();
app.UseAuthorization();
app.MapControllers();
app.Run();