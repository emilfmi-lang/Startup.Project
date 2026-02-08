using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Startup_Project.Domain.Entities;
using Startup_Project.Domain.Entities.Identity;
using Startup_Project.Domain.Enums;
using Startup_Project.Persistance.Data;

namespace Startup_Project.Persistance.Seeding;

public static class UserSubscriptionSeeder
{
    public static async Task SeedSubscriptionsAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();

        // Check if subscriptions already exist
        if (await context.Subscriptions.AnyAsync())
            return;

        // Get users and packages
        var users = await userManager.GetUsersInRoleAsync("User");
        var packages = await context.Packages
            .Include(p => p.Gym)
            .ToListAsync();

        if (users.Count == 0 || packages.Count == 0)
            return;

        var subscriptions = new List<Subscription>();
        var payments = new List<Payment>();
        var now = DateTime.UtcNow;

        // Get packages by price (more reliable than name)
        var premiumPackages = packages.Where(p => p.Price == 80).ToList();
        var standardPackages = packages.Where(p => p.Price == 50).ToList();
        var quarterlyPackages = packages.Where(p => p.Price == 130).ToList();
        var yearlyPackages = packages.Where(p => p.Price == 400).ToList();

        // User 1: 2 active subscriptions (different gyms)
        var user1 = users.FirstOrDefault(u => u.Email == "user1@fitnessapp.az");
        if (user1 != null && premiumPackages.Count > 0 && standardPackages.Count > 1)
        {
            // Active subscription 1 - Premium (18 days left)
            var package1 = premiumPackages[0];
            var sub1 = new Subscription(user1.Id, package1, now.AddDays(-12));
            subscriptions.Add(sub1);

            var payment1 = new Payment(user1.Id, package1.Id, package1.Price, $"session_{Guid.NewGuid():N}");
            payment1.MarkAsSuccess();
            payments.Add(payment1);

            // Active subscription 2 - Standard (different gym, 5 days left - expiring soon)
            var package2 = standardPackages.FirstOrDefault(p => p.GymId != package1.GymId) ?? standardPackages[1];
            var sub2 = new Subscription(user1.Id, package2, now.AddDays(-25));
            subscriptions.Add(sub2);

            var payment2 = new Payment(user1.Id, package2.Id, package2.Price, $"session_{Guid.NewGuid():N}");
            payment2.MarkAsSuccess();
            payments.Add(payment2);
        }

        // User 2: 1 active subscription + 1 expired (history)
        var user2 = users.FirstOrDefault(u => u.Email == "user2@fitnessapp.az");
        if (user2 != null && quarterlyPackages.Count > 0 && standardPackages.Count > 0)
        {
            // Active subscription - 3 monthly
            var package3 = quarterlyPackages[0];
            var sub3 = new Subscription(user2.Id, package3, now.AddDays(-30));
            subscriptions.Add(sub3);

            var payment3 = new Payment(user2.Id, package3.Id, package3.Price, $"session_{Guid.NewGuid():N}");
            payment3.MarkAsSuccess();
            payments.Add(payment3);

            // Expired subscription (for history)
            var package4 = standardPackages[0];
            var sub4 = new Subscription(user2.Id, package4, now.AddDays(-60));
            subscriptions.Add(sub4);

            var payment4 = new Payment(user2.Id, package4.Id, package4.Price, $"session_{Guid.NewGuid():N}");
            payment4.MarkAsSuccess();
            payments.Add(payment4);
        }

        // User 3: 1 active subscription (yearly)
        var user3 = users.FirstOrDefault(u => u.Email == "user3@fitnessapp.az");
        if (user3 != null && yearlyPackages.Count > 0)
        {
            var package5 = yearlyPackages[0];
            var sub5 = new Subscription(user3.Id, package5, now.AddDays(-100));
            subscriptions.Add(sub5);

            var payment5 = new Payment(user3.Id, package5.Id, package5.Price, $"session_{Guid.NewGuid():N}");
            payment5.MarkAsSuccess();
            payments.Add(payment5);
        }

        // User 4: Multiple history records
        var user4 = users.FirstOrDefault(u => u.Email == "user4@fitnessapp.az");
        if (user4 != null && premiumPackages.Count > 0 && standardPackages.Count > 1)
        {
            // Current active
            var package6 = premiumPackages.Count > 1 ? premiumPackages[1] : premiumPackages[0];
            var sub6 = new Subscription(user4.Id, package6, now.AddDays(-5));
            subscriptions.Add(sub6);

            var payment6 = new Payment(user4.Id, package6.Id, package6.Price, $"session_{Guid.NewGuid():N}");
            payment6.MarkAsSuccess();
            payments.Add(payment6);

            // History 1 (2 months ago)
            var package7 = standardPackages[0];
            var sub7 = new Subscription(user4.Id, package7, now.AddDays(-90));
            subscriptions.Add(sub7);

            var payment7 = new Payment(user4.Id, package7.Id, package7.Price, $"session_{Guid.NewGuid():N}");
            payment7.MarkAsSuccess();
            payments.Add(payment7);

            // History 2 (3 months ago)
            var package8 = standardPackages.Count > 1 ? standardPackages[1] : standardPackages[0];
            var sub8 = new Subscription(user4.Id, package8, now.AddDays(-120));
            subscriptions.Add(sub8);

            var payment8 = new Payment(user4.Id, package8.Id, package8.Price, $"session_{Guid.NewGuid():N}");
            payment8.MarkAsSuccess();
            payments.Add(payment8);
        }

        // Save all subscriptions and payments
        if (subscriptions.Count > 0)
        {
            await context.Subscriptions.AddRangeAsync(subscriptions);
            await context.SaveChangesAsync();
        }

        if (payments.Count > 0)
        {
            await context.Payments.AddRangeAsync(payments);
            await context.SaveChangesAsync();
        }
    }
}
    
