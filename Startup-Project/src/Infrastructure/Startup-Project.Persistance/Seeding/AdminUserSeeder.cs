using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Startup_Project.Domain.Entities.Identity;

namespace Startup_Project.Persistance.Seeding;

public static class AdminUserSeeder
{
    public static async Task SeedAdminAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();

        string adminEmail = "admin@fitnessapp.az";
        string adminPassword = "Admin123!";

        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
        
        if (existingAdmin == null)
        {
            var admin = new AppUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "System Administrator",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, adminPassword);
            
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }
    }
}
