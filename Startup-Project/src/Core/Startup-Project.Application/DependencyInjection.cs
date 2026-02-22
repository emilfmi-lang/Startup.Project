using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Startup_Project.Application.Services.Concretes;
using Startup_Project.Application.Services.Concretes.Account;
using Startup_Project.Application.Services.Concretes.Account.Settings;
using Startup_Project.Application.Services.Concretes.Admin;
using Startup_Project.Application.Services.Concretes.RuleHandlers;
using Startup_Project.Application.Services.Interfaces;
using Startup_Project.Application.Services.Interfaces.Account;
using Startup_Project.Application.Services.Interfaces.Account.Settings;
using Startup_Project.Application.Services.Interfaces.Admin;

namespace Startup_Project.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => cfg.AddMaps(typeof(DependencyInjection).Assembly));
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // Account
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<ISettingsService, SettingsService>();

        // Gym
        services.AddScoped<IGymService, GymService>();
        services.AddScoped<IGymOpeningHourService, GymOpeningHourService>();
        services.AddScoped<IGymOwnerService, GymOwnerService>();

        // Package & Payment
        services.AddScoped<IPackageService, PackageService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<ISessionPaymentService, SessionPaymentService>();

        // Review & Favorite
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IFavoriteService, FavoriteService>();

        // Trainer & Session
        services.AddScoped<ITrainerService, TrainerService>();
        services.AddScoped<ITrainingSessionService, TrainingSessionService>();
        services.AddScoped<ITrainerSessionService, TrainerSessionService>();
        services.AddScoped<ITrainerStudentService, TrainerStudentService>();
        services.AddScoped<ITrainerScheduleService, TrainerScheduleService>();

        // History
        services.AddScoped<IHistoryService, HistoryService>();

        // PromoCode
        services.AddScoped<IPromoCodeService, PromoCodeService>();

        // User Dashboard
        services.AddScoped<IUserSubscriptionService, UserSubscriptionService>();

        // Admin
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IPromoCodeAdminService, PromoCodeAdminService>();
        services.AddScoped<IGymAdminMembersService, GymAdminMembersService>();
        services.AddScoped<IGymAdminPanelService, GymAdminPanelService>();
        services.AddScoped<IGymAdminSessionService, GymAdminSessionService>();

        // Campaign & Pricing - Rule Handlers (Strategy Pattern)
        services.AddScoped<IRuleHandler, DayOfWeekRuleHandler>();
        services.AddScoped<IRuleHandler, TimeRangeRuleHandler>();
        services.AddScoped<IRuleHandler, UserSegmentRuleHandler>();
        services.AddScoped<IRuleHandler, PackageRuleHandler>();
        services.AddScoped<IRuleHandler, MinimumOrderAmountRuleHandler>();

        services.AddScoped<IRuleEngine, RuleEngine>();
        services.AddScoped<IPricingEngine, PricingEngine>();

        services.AddScoped<ICampaignService, CampaignService>();
        services.AddScoped<ICampaignAdminService, CampaignAdminService>();

        return services;
    }
}
