using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Startup_Project.Application.Abstractions.AI;
using Startup_Project.Application.Abstractions.Chat;
using Startup_Project.Application.Abstractions.Services;
using Startup_Project.Application.Interfaces;
using Startup_Project.Application.Services.Interfaces;
using Startup_Project.Application.Services.Interfaces.Account;
using Startup_Project.Infrastructure.Concretes.Services;
using Startup_Project.Infrastructure.Concretes.Services.AI;
using Startup_Project.Infrastructure.Concretes.Services.Chat;
using Startup_Project.Infrastructure.Security;

namespace Startup_Project.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IMailService, MailService>();
        services.AddScoped<ITokenHandler, TokenHandler>();
        services.AddScoped<IFileService, FileService>();

        // Bank Service — Mock v? ya Real KapitalBank
        var useMockBank = configuration.GetValue<bool>("KapitalBank:UseMock");
        if (useMockBank)
            services.AddSingleton<IBankService, MockBankService>();
        else
            services.AddHttpClient<IBankService, KapitalBankService>();

        services.AddScoped<IQrTokenService, QrTokenService>();
        services.AddScoped<IGoogleTokenValidator, GoogleTokenValidator>();
        services.AddScoped<ISmsService, ConsoleSmsService>();

        services.AddScoped<IGoogleAuthService, GoogleAuthService>();

        // Chat — Presence Tracker (Singleton)
        services.AddSingleton<IPresenceTracker, InMemoryPresenceTracker>();

        // AI — Gemini Provider
        services.Configure<GeminiSettings>(configuration.GetSection(GeminiSettings.SectionName));
        services.AddScoped<IAiProvider, GeminiProvider>();

        return services;
    }
}
