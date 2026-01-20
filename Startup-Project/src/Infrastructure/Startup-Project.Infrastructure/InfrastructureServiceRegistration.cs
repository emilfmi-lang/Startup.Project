using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Startup_Project.Application.Abstractions.Services;
using Startup_Project.Infrastructure.Concretes.Services;
using Startup_Project.Infrastructure.Security;

namespace Startup_Project.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IMailService, MailService>();
        services.AddScoped<ITokenHandler, TokenHandler>();
        return services;
    }
}
