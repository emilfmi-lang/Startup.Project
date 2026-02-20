
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using Startup_Project.Application;
using Startup_Project.Application.Interfaces;
using Startup_Project.Infrastructure;
using Startup_Project.Infrastructure.Concretes.Services;
using Startup_Project.Persistance;
using Startup_Project.WebAPI.Middlewares;
using System.Security.Cryptography.X509Certificates;
using System.Text;

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

    builder.Services.AddHttpContextAccessor();

    builder.Services.AddApplicationServices();
    builder.Services.AddPersistanceServices(builder.Configuration);
    builder.Services.AddInfrastructureServices(builder.Configuration);
    builder.Services.AddHttpClient<IBankService, KapitalBankService>()
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        var handler = new HttpClientHandler();
        handler.ClientCertificates.Add(new X509Certificate2("path_to_cert.pfx", "password"));
        return handler;
    });

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]!))
        };
    });

    builder.Services.AddAuthorization();

    builder.Services.AddControllers();

    builder.Services.AddOpenApi();

    builder.Services.AddCors(opt =>
    {
        opt.AddPolicy("CorsPolicy", policy =>
        {
            policy.WithOrigins("http://localhost:5173", "http://localhost:8080")
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
    });

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        await Startup_Project.Persistance.Seeding.RoleSeeder.SeedRolesAsync(services);
        await Startup_Project.Persistance.Seeding.AdminUserSeeder.SeedAdminAsync(services);
        await Startup_Project.Persistance.Seeding.GymSeeder.SeedGymsAsync(services);
    }

    app.UseMiddleware<ExceptionHandlingMiddleware>();

    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "Startup Project API";
        options.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });

    app.UseStaticFiles();

    app.UseCors("CorsPolicy");

    app.UseAuthentication();
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