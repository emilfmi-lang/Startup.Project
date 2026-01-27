using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Startup_Project.Application.Services.Concretes;
using Startup_Project.Application.Services.Concretes.Account;
using Startup_Project.Application.Services.Interfaces;
using Startup_Project.Application.Services.Interfaces.Account;


namespace Startup_Project.Application;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public void AddApplicationServices()
        {
            services.AddAutoMapper(cfg => cfg.AddMaps(typeof(DependencyInjection).Assembly));
            services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IGymService, GymService>();
            services.AddScoped<IGymOpeningHourService, GymOpeningHourService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IPackageService, PackageService>();
            services.AddScoped<IReviewService, ReviewService>();
        }
    }
}
