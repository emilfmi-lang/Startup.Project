
using Scalar.AspNetCore;
using Serilog;
using Startup_Project.Application;
using Startup_Project.Application.Interfaces;
using Startup_Project.Infrastructure;
using Startup_Project.Infrastructure.Concretes.Services;
using Startup_Project.Persistance;
using Startup_Project.WebAPI.Middlewares;
using System.Security.Cryptography.X509Certificates;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/app_log-.txt",
    rollingInterval: RollingInterval.Day,
    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Proqram başladılır...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    builder.Services.AddApplicationServices();
    builder.Services.AddPersistanceServices(builder.Configuration);
    builder.Services.AddInfrastructureServices(builder.Configuration);
    builder.Services.AddHttpClient<IBankService, KapitalBankService>()
    .ConfigurePrimaryHttpMessageHandler(() => {
        var handler = new HttpClientHandler();
        handler.ClientCertificates.Add(new X509Certificate2("path_to_cert.pfx", "password"));
        return handler;
    });

    builder.Services.AddControllers();

    builder.Services.AddOpenApi();

    var app = builder.Build();
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "Startup Project API";
        options.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });

    app.UseStaticFiles();

    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Proqramın işə düşməsi zamanı kritik xəta baş verdi!");
}
finally
{
    Log.CloseAndFlush();
}