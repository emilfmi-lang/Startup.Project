using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Startup_Project.Domain.Entities;
using Startup_Project.Domain.Enums;
using Startup_Project.Persistance.Data;

namespace Startup_Project.Persistance.Seeding;

public static class PromoCodeSeeder
{
    public static async Task SeedPromoCodesAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

        if (await context.PromoCodes.AnyAsync(p => !p.IsDeleted))
            return;

        var admin = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@fitnessapp.az");
        if (admin is null)
            return;

        var promoCodes = new List<PromoCode>
        {
            new(
                code: "FIT20",
                description: "20% endirim — bütün paketl?r üçün keç?rli",
                discountType: DiscountType.Percentage,
                discountValue: 20,
                targetType: PromoCodeTargetType.Package,
                startDate: DateTime.UtcNow,
                endDate: DateTime.UtcNow.AddYears(1),
                usageLimit: 1000,
                createdById: admin.Id
            ),
            new(
                code: "FIT50",
                description: "50% endirim — bütün paketl?r üçün keç?rli",
                discountType: DiscountType.Percentage,
                discountValue: 50,
                targetType: PromoCodeTargetType.Package,
                startDate: DateTime.UtcNow,
                endDate: DateTime.UtcNow.AddYears(1),
                usageLimit: 500,
                createdById: admin.Id
            )
        };

        context.PromoCodes.AddRange(promoCodes);
        await context.SaveChangesAsync();
    }
}
