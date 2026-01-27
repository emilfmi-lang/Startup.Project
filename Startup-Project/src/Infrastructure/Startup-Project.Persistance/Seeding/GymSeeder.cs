using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Startup_Project.Domain.Entities;
using Startup_Project.Persistance.Data;

namespace Startup_Project.Persistance.Seeding;

public static class GymSeeder
{
    public static async Task SeedGymsAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

        if (await context.Gyms.AnyAsync())
            return;

        var gyms = new List<Gym>
        {
            new("FitLife Gym", "Baki, Nerimanov rayonu, Ataturk prospekti 45"),
            new("PowerHouse Fitness", "Baki, Yasamal rayonu, Insaatcilar prospekti 78"),
            new("IronMan Gym", "Baki, Nasimi rayonu, Nizami kucesi 112"),
            new("FlexZone Studio", "Baki, Xetai rayonu, Babek prospekti 23"),
            new("Elite Sports Club", "Baki, Sebail rayonu, Neftciler prospekti 156")
        };

        await context.Gyms.AddRangeAsync(gyms);
        await context.SaveChangesAsync();
    }
}
