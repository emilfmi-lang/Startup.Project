using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Startup_Project.Domain.Entities;
using Startup_Project.Domain.Entities.Identity;
using Startup_Project.Persistance.Data;

namespace Startup_Project.Persistance.Seeding;

/// <summary>
/// Admin Panel Test Data Seeder
/// Yeni GymOwner + 5 Members + 5 Trainers üçün
/// </summary>
public static class AdminPanelTestDataSeeder
{
    // IDs from Admin Panel Requirements
    private static readonly Guid GYM_OWNER_ID = Guid.Parse("799d8189-c97d-4cc2-f182-08de68b50fea");
    private static readonly Guid GYM_ID = Guid.Parse("a60ead4a-4bbc-4323-9525-6eccc2b85599");

    public static async Task SeedAdminPanelTestDataAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();

        // 1. Create GymOwner User
        await CreateGymOwnerAsync(userManager);

        // 2. Create Gym with GymOwner
        await CreateGymAsync(context, userManager);

        // 3. Create 5 Members
        await CreateMembersAsync(context, userManager);

        // 4. Create 5 Trainers
        await CreateTrainersAsync(context, userManager);

        // 5. Create Subscriptions for Members
        await CreateSubscriptionsAsync(context, userManager);

        // 6. Create CheckIn Logs for Members
        await CreateCheckInLogsAsync(context, userManager);
    }

    /// <summary>
    /// Create GymOwner admin user
    /// Email: gymowner@fitnessapp.az
    /// Password: GymOwner123!
    /// </summary>
    private static async Task CreateGymOwnerAsync(UserManager<AppUser> userManager)
    {
        // Check if already exists
        var existingOwner = await userManager.FindByIdAsync(GYM_OWNER_ID.ToString());
        if (existingOwner != null)
        {
            // Just ensure role is assigned
            var roles = await userManager.GetRolesAsync(existingOwner);
            if (!roles.Contains("GymOwner"))
            {
                await userManager.AddToRoleAsync(existingOwner, "GymOwner");
            }
            return;
        }

        var owner = new AppUser
        {
            Id = GYM_OWNER_ID,
            UserName = "gymowner@fitnessapp.az",
            Email = "gymowner@fitnessapp.az",
            FullName = "FitLife Admin",
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow,
            PhoneNumber = "+994505551234"
        };

        var result = await userManager.CreateAsync(owner, "GymOwner123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(owner, "GymOwner");
        }
    }

    /// <summary>
    /// Create Gym for admin panel
    /// Name: FitLife Premium Gym
    /// Owner: GYM_OWNER_ID
    /// </summary>
    private static async Task CreateGymAsync(ApplicationDbContext context, UserManager<AppUser> userManager)
    {
        var existingGym = await context.Gyms.FindAsync(GYM_ID);
        if (existingGym != null)
            return;

        var owner = await userManager.FindByIdAsync(GYM_OWNER_ID.ToString());
        if (owner == null)
            return;

        var gym = new Gym(
            "FitLife Premium Gym",
            "123 Fitness Street, Baku, Azerbaijan",
            GYM_OWNER_ID,
            null
        );

        // Manually set ID since we need specific ID
        var property = gym.GetType().GetProperty("Id");
        property?.SetValue(gym, GYM_ID);

        context.Gyms.Add(gym);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Create 5 Members for the gym
    /// Members will have GymId = GYM_ID
    /// </summary>
    private static async Task CreateMembersAsync(ApplicationDbContext context, UserManager<AppUser> userManager)
    {
        var memberEmails = new[]
        {
            ("aysel@email.com", "Aysel Mammadova"),
            ("kamran@email.com", "Kamran Hasanov"),
            ("nigar@email.com", "Nigar Aliyeva"),
            ("rashad@email.com", "Rashad Guliyev"),
            ("leyla@email.com", "Leyla Huseynova")
        };

        foreach (var (email, fullName) in memberEmails)
        {
            var existing = await userManager.FindByEmailAsync(email);
            if (existing != null)
            {
                // Update GymId if not set
                if (existing.GymId != GYM_ID)
                {
                    existing.GymId = GYM_ID;
                    await userManager.UpdateAsync(existing);
                }
                continue;
            }

            var member = new AppUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                GymId = GYM_ID,
                PhoneNumber = $"+9945055512{new Random().Next(10, 99)}"
            };

            var result = await userManager.CreateAsync(member, "Member123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(member, "User");
            }
        }
    }

    /// <summary>
    /// Create 5 Trainers for the gym
    /// Trainers will have GymId = GYM_ID
    /// </summary>
    private static async Task CreateTrainersAsync(ApplicationDbContext context, UserManager<AppUser> userManager)
    {
        var trainerData = new[]
        {
            ("elvin@email.com", "Elvin Mammadov", "Personal Training", 50, "Professional strength coach with 5+ years experience"),
            ("sabina@email.com", "Sabina Aliyeva", "Yoga & Pilates", 45, "Certified yoga instructor with calming approach"),
            ("tural@email.com", "Tural Hasanov", "Strength Training", 55, "Bodybuilding champion, competitive training specialist"),
            ("gunel@email.com", "Gunel Huseynova", "Cardio & HIIT", 40, "High-intensity training expert for weight loss"),
            ("samir@email.com", "Samir Aliyev", "Boxing & Fitness", 50, "Professional boxer turned fitness coach")
        };

        var gym = await context.Gyms.FindAsync(GYM_ID);
        if (gym == null)
            return;

        foreach (var (email, fullName, specialization, sessionPrice, bio) in trainerData)
        {
            var existing = await userManager.FindByEmailAsync(email);
            AppUser trainerUser;

            if (existing == null)
            {
                trainerUser = new AppUser
                {
                    UserName = email,
                    Email = email,
                    FullName = fullName,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    GymId = GYM_ID,
                    PhoneNumber = $"+9945055512{new Random().Next(10, 99)}"
                };

                var result = await userManager.CreateAsync(trainerUser, "Trainer123!");
                if (!result.Succeeded)
                    continue;

                await userManager.AddToRoleAsync(trainerUser, "Trainer");
            }
            else
            {
                trainerUser = existing;
            }

            // Check if trainer profile already exists
            var existingTrainer = await context.Trainers
                .FirstOrDefaultAsync(t => t.UserId == trainerUser.Id && t.GymId == GYM_ID);

            if (existingTrainer == null)
            {
                var trainer = new Trainer(
                    trainerUser.Id,
                    GYM_ID,
                    specialization,
                    (decimal)sessionPrice
                );

                trainer.SetBio(bio);
                trainer.Activate();

                context.Trainers.Add(trainer);
            }
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Create Subscriptions for all members
    /// Members will have Premium Monthly subscriptions
    /// </summary>
    private static async Task CreateSubscriptionsAsync(ApplicationDbContext context, UserManager<AppUser> userManager)
    {
        var memberEmails = new[]
        {
            "aysel@email.com",
            "kamran@email.com",
            "nigar@email.com",
            "rashad@email.com",
            "leyla@email.com"
        };

        var premiumPlan = await context.Packages
            .FirstOrDefaultAsync(p => p.Name == "Premium Monthly" && p.GymId == GYM_ID);

        if (premiumPlan == null)
        {
            // Create Premium Monthly package if doesn't exist
            premiumPlan = new Package("Premium Monthly", 99.99m, 30, GYM_ID);
            context.Packages.Add(premiumPlan);
            await context.SaveChangesAsync();
        }

        var random = new Random();

        foreach (var email in memberEmails)
        {
            var member = await userManager.FindByEmailAsync(email);
            if (member == null)
                continue;

            var existingSubscription = await context.Subscriptions
                .FirstOrDefaultAsync(s => s.UserId == member.Id && s.PackageId == premiumPlan.Id);

            if (existingSubscription == null)
            {
                var startDate = DateTime.UtcNow.AddMonths(-random.Next(1, 6));
                var subscription = new Subscription(member.Id, premiumPlan, startDate);

                context.Subscriptions.Add(subscription);
            }
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Create CheckIn Logs for members
    /// Each member will have 20-50 check-ins
    /// </summary>
    private static async Task CreateCheckInLogsAsync(ApplicationDbContext context, UserManager<AppUser> userManager)
    {
        var memberEmails = new[]
        {
            ("aysel@email.com", 45),
            ("kamran@email.com", 32),
            ("nigar@email.com", 28),
            ("rashad@email.com", 38),
            ("leyla@email.com", 42)
        };

        var random = new Random();

        foreach (var (email, checkInCount) in memberEmails)
        {
            var member = await userManager.FindByEmailAsync(email);
            if (member == null || member.GymId != GYM_ID)
                continue;

            var existingLogs = await context.CheckInLogs
                .Where(c => c.UserId == member.Id && c.GymId == GYM_ID)
                .CountAsync();

            if (existingLogs >= checkInCount)
                continue;

            var logsToAdd = checkInCount - existingLogs;

            for (int i = 0; i < logsToAdd; i++)
            {
                var checkInDate = DateTime.UtcNow
                    .AddDays(-random.Next(0, 180))
                    .AddHours(random.Next(6, 22));

                var log = new CheckInLog(member.Id, GYM_ID);

                // Set the CheckInTime via reflection
                var checkInTimeProperty = log.GetType().GetProperty("CheckInTime");
                checkInTimeProperty?.SetValue(log, checkInDate);

                // Set CreatedAt via reflection
                var createdAtProperty = log.GetType().GetProperty("CreatedAt");
                createdAtProperty?.SetValue(log, checkInDate);

                context.CheckInLogs.Add(log);
            }
        }

        await context.SaveChangesAsync();
    }
}
