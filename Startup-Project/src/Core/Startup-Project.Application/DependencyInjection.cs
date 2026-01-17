using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Startup_Project.Application.Mappings.Account;
using Startup_Project.Application.Mappings.Gyms;
using Startup_Project.Application.Services.Concretes.Account;
using Startup_Project.Application.Services.Interfaces;


namespace Startup_Project.Application;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public void AddApplicationServices()
        {
            services.AddAutoMapper(cfg => { cfg.AddProfile<AccountProfile>(); });
            services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
            services.AddScoped<IAccountService, AccountService>();


        }
    }
}
