using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Startup_Project.Application.Services.Concretes;
using Startup_Project.Application.Services.Concretes.Account;
using Startup_Project.Application.Services.Concretes.Admin;
using Startup_Project.Application.Services.Concretes.RuleHandlers;
using Startup_Project.Application.Services.Interfaces;
using Startup_Project.Application.Services.Interfaces.Account;
using Startup_Project.Application.Services.Interfaces.Admin;

namespace Startup_Project.Application;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public void AddApplicationServices()
        {
            services.AddAutoMapper(cfg => cfg.AddMaps(typeof(DependencyInjection).Assembly));
            services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

            // Account
            services.AddScoped<IAccountService, AccountService>();

            // Gym
            services.AddScoped<IGymService, GymService>();
            services.AddScoped<IGymOpeningHourService, GymOpeningHourService>();
            services.AddScoped<IGymOwnerService, GymOwnerService>();

            // Package & Payment
            services.AddScoped<IPackageService, PackageService>();
            services.AddScoped<IPaymentService, PaymentService>();

            // Review & Favorite
            services.AddScoped<IReviewService, ReviewService>();
            services.AddScoped<IFavoriteService, FavoriteService>();

            // Trainer & Session
            services.AddScoped<ITrainerService, TrainerService>();
            services.AddScoped<ITrainingSessionService, TrainingSessionService>();

            // History
            services.AddScoped<IHistoryService, HistoryService>();

            // PromoCode
            services.AddScoped<IPromoCodeService, PromoCodeService>();

            // Admin
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<IPromoCodeAdminService, PromoCodeAdminService>();

            // Campaign & Pricing - Rule Handlers (Strategy Pattern)
            services.AddScoped<IRuleHandler, DayOfWeekRuleHandler>();
            services.AddScoped<IRuleHandler, TimeRangeRuleHandler>();
            services.AddScoped<IRuleHandler, UserSegmentRuleHandler>();
            services.AddScoped<IRuleHandler, PackageRuleHandler>();
            services.AddScoped<IRuleHandler, MinimumOrderAmountRuleHandler>();

            // Campaign & Pricing - Engines
            services.AddScoped<IRuleEngine, RuleEngine>();
            services.AddScoped<IPricingEngine, PricingEngine>();

            // Campaign & Pricing - Services
            services.AddScoped<ICampaignService, CampaignService>();
            services.AddScoped<ICampaignAdminService, CampaignAdminService>();
        }
    }
}
