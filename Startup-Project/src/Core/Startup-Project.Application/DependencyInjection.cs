using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Startup_Project.Application.Mappings.Gyms;
using System.Reflection;
using FluentValidation;


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
