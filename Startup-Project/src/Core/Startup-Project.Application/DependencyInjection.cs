using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Startup_Project.Application.Mappings.Gyms;


namespace Startup_Project.Application;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public void AddApplicationServices()
        {
            services.AddAutoMapper(cfg => { cfg.AddProfile<GymProfile>(); });
            services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        }
    }
}
