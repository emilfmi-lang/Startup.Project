
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Startup_Project.Application.Interfaces;
using Startup_Project.Persistance.Data;
using Microsoft.AspNetCore.Identity;
using Startup_Project.Domain.Entities.Identity;
using Startup_Project.Application.Services.Concretes.Account;
using Startup_Project.Application.Services.Interfaces.Account;

namespace Startup_Project.Persistance;

public static class PersistenceServiceRegistration
{
    public static IServiceCollection AddPersistanceServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        services.AddScoped<IApplicationDbContext, ApplicationDbContext>();
        services.AddIdentity<AppUser, IdentityRole<Guid>>(options=>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
            options.User.RequireUniqueEmail = true; 
        }) .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<IAccountService, AccountService> ();
        return services;
    }
}
