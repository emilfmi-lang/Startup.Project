
using Serilog;
using Scalar.AspNetCore;
using Startup_Project.Application;
using Startup_Project.Persistance;
using Startup_Project.Infrastructure;
using Startup_Project.WebAPI.Middlewares;

// 1. Serilog-u hər şeydən əvvəl konfiqurasiya edirik
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

    // 2. Serilog-u .NET-in daxili log sistemi ilə əvəz edirik
    builder.Host.UseSerilog();

    // Servis qeydiyyatları
    builder.Services.AddApplicationServices();
    builder.Services.AddPersistanceServices(builder.Configuration);
    builder.Services.AddInfrastructureServices(builder.Configuration);

    builder.Services.AddControllers();

    builder.Services.AddOpenApi();

    var app = builder.Build();

    // 3. Xəta tutan Middleware (Mütləq MapOpenApi-dən əvvəl olmalıdır ki, hər şeyi tutsun)
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
    // Əgər proqram açılmasa, səbəbi burada (və log faylında) görünəcək
    Log.Fatal(ex, "Proqramın işə düşməsi zamanı kritik xəta baş verdi!");
}
finally
{
    // Yaddaşdakı logları təmizləyib bağlayırıq
    Log.CloseAndFlush();
}