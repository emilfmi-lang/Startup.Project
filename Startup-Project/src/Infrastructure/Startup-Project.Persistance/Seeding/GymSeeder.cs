using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Startup_Project.Domain.Entities;
using Startup_Project.Domain.Entities.Identity;
using Startup_Project.Persistance.Data;

namespace Startup_Project.Persistance.Seeding;

public static class GymSeeder
{
    public static async Task SeedGymsAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();

        // 1. Seed Gyms
        if (!await context.Gyms.AnyAsync())
        {
            var gyms = new List<Gym>
            {
                new("FitLife Gym", "Bak?, N?rimanov rayonu, Atatürk prospekti 45"),
                new("PowerHouse Fitness", "Bak?, Yasamal rayonu, ?n?aatç?lar prospekti 78"),
                new("IronMan Gym", "Bak?, N?simi rayonu, Nizami küç?si 112"),
                new("FlexZone Studio", "Bak?, X?tai rayonu, Bab?k prospekti 23")
            };

            await context.Gyms.AddRangeAsync(gyms);
            await context.SaveChangesAsync();
        }

        // 2. Seed GymOwners (4 user)
        var gymOwnerEmails = new[]
            {
                "owner1@fitnessapp.az",
                "owner2@fitnessapp.az",
                "owner3@fitnessapp.az",
                "owner4@fitnessapp.az"
            };

        var gymOwnerNames = new[]
        {
            "El??n M?mm?dov",
            "Nigar H?s?nova",
            "R??ad ?liyev",
            "Günel Quliyeva"
        };

        var gymOwners = new List<AppUser>();

        for (int i = 0; i < gymOwnerEmails.Length; i++)
        {
            var existingUser = await userManager.FindByEmailAsync(gymOwnerEmails[i]);
            if (existingUser == null)
            {
                var owner = new AppUser
                {
                    UserName = gymOwnerEmails[i],
                    Email = gymOwnerEmails[i],
                    FullName = gymOwnerNames[i],
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(owner, "Owner123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(owner, "GymOwner");
                    gymOwners.Add(owner);
                }
            }
                else
                {
                    if (!await userManager.IsInRoleAsync(existingUser, "GymOwner"))
                        await userManager.AddToRoleAsync(existingUser, "GymOwner");

                    gymOwners.Add(existingUser);
                }
            }

            // 3. Assign Gyms to Owners
        var allGyms = await context.Gyms.ToListAsync();
        for (int i = 0; i < allGyms.Count && i < gymOwners.Count; i++)
        {
            if (allGyms[i].OwnerId == null)
            {
                allGyms[i].SetOwner(gymOwners[i].Id);
            }
        }
        await context.SaveChangesAsync();

        // 4. Seed Regular Users (4 user)
        var userEmails = new[]
        {
            "user1@fitnessapp.az",
            "user2@fitnessapp.az",
            "user3@fitnessapp.az",
            "user4@fitnessapp.az"
        };

        var userNames = new[]
        {
            "Tural ?hm?dov",
            "Leyla Hüseynova",
            "Orxan S?f?rov",
            "Aynur Kaz?mova"
        };

        var regularUsers = new List<AppUser>();

        for (int i = 0; i < userEmails.Length; i++)
        {
            var existingUser = await userManager.FindByEmailAsync(userEmails[i]);
            if (existingUser == null)
            {
                var user = new AppUser
                {
                    UserName = userEmails[i],
                    Email = userEmails[i],
                    FullName = userNames[i],
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, "User123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "User");
                    regularUsers.Add(user);
                }
            }
                else
                {
                    if (!await userManager.IsInRoleAsync(existingUser, "User"))
                        await userManager.AddToRoleAsync(existingUser, "User");

                    regularUsers.Add(existingUser);
                }
            }

            // 5. Seed Trainer Users
        var trainerEmails = new[]
        {
            "trainer1@fitnessapp.az",
            "trainer2@fitnessapp.az",
            "trainer3@fitnessapp.az",
            "trainer4@fitnessapp.az"
        };

        var trainerNames = new[]
        {
            "Kamran N?sirov",
            "S?bin? M?mm?dli",
            "Vüsal H?s?nli",
            "Lamiy? Rüst?mova"
        };

        var trainerUsers = new List<AppUser>();

        for (int i = 0; i < trainerEmails.Length; i++)
        {
            var existingUser = await userManager.FindByEmailAsync(trainerEmails[i]);
            if (existingUser == null)
            {
                var user = new AppUser
                {
                    UserName = trainerEmails[i],
                    Email = trainerEmails[i],
                    FullName = trainerNames[i],
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, "Trainer123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Trainer");
                    trainerUsers.Add(user);
                }
            }
                else
                {
                    if (!await userManager.IsInRoleAsync(existingUser, "Trainer"))
                        await userManager.AddToRoleAsync(existingUser, "Trainer");

                    trainerUsers.Add(existingUser);
                }
            }

            // 6. Seed Trainers
        if (!await context.Trainers.AnyAsync())
        {
            var gyms = await context.Gyms.ToListAsync();

            var trainers = new List<Trainer>();

            // Gym-based Trainers (first 2)
            if (trainerUsers.Count >= 2 && gyms.Count >= 2)
            {
                // Trainer 1 - Gym-based (FitLife Gym)
                var trainer1 = new Trainer(trainerUsers[0].Id, gyms[0].Id, "Bodybuilding, Strength Training", 30);
                trainer1.SetBio("10 illik t?crüb?y? malik pe??kar bodybuilding m??qçisi. Az?rbaycan çempionu.");
                trainer1.Activate();
                trainers.Add(trainer1);

                // Trainer 2 - Gym-based (PowerHouse Fitness)
                var trainer2 = new Trainer(trainerUsers[1].Id, gyms[1].Id, "Cardio, Weight Loss", 25);
                trainer2.SetBio("Kilo verm? v? kardio üzr? müt?x?ssis. 500+ mü?t?ri il? i?l?mi??m.");
                trainer2.Activate();
                trainers.Add(trainer2);
            }

            // Online Trainers (last 2)
            if (trainerUsers.Count >= 4)
            {
                // Trainer 3 - Online
                var trainer3 = new Trainer(trainerUsers[2].Id, "Yoga, Pilates", 20);
                trainer3.SetBio("Sertifikatl? yoga v? pilates m??qçisi. Online d?rsl?r üzr? ixtisasla?m??am.");
                trainer3.SetVideoIntroUrl("https://youtube.com/watch?v=demo1");
                trainer3.SetSocialMediaLinks("{\"instagram\":\"@trainer_vusal\",\"youtube\":\"TrainerVusal\"}");
                trainer3.Verify();
                trainers.Add(trainer3);

                // Trainer 4 - Online
                var trainer4 = new Trainer(trainerUsers[3].Id, "CrossFit, HIIT", 35);
                trainer4.SetBio("CrossFit Level 2 sertifikat?. Yüks?k intensivlikli m??ql?r üzr? müt?x?ssis.");
                trainer4.SetVideoIntroUrl("https://youtube.com/watch?v=demo2");
                trainer4.SetSocialMediaLinks("{\"instagram\":\"@lamiya_fit\",\"tiktok\":\"@lamiya_crossfit\"}");
                trainer4.Verify();
                trainers.Add(trainer4);
            }

            await context.Trainers.AddRangeAsync(trainers);
            await context.SaveChangesAsync();
        }

        // 7. Seed Packages
        if (!await context.Packages.AnyAsync())
        {
            var gyms = await context.Gyms.ToListAsync();
            var packages = new List<Package>();

            foreach (var gym in gyms)
            {
                packages.Add(new Package("Ayl?q Standart", 50, 30, gym.Id));
                packages.Add(new Package("Ayl?q Premium", 80, 30, gym.Id));
                packages.Add(new Package("3 Ayl?q", 130, 90, gym.Id));
                packages.Add(new Package("?llik", 400, 365, gym.Id));
            }

            await context.Packages.AddRangeAsync(packages);
            await context.SaveChangesAsync();
        }

        // 8. Seed Opening Hours
        if (!await context.GymOpeningHours.AnyAsync())
        {
            var gyms = await context.Gyms.ToListAsync();
            var openingHours = new List<GymOpeningHour>();

            foreach (var gym in gyms)
            {
                // Monday - Friday: 06:00 - 23:00
                for (var day = DayOfWeek.Monday; day <= DayOfWeek.Friday; day++)
                {
                    openingHours.Add(new GymOpeningHour
                    {
                        GymId = gym.Id,
                        DayOfWeek = day,
                        OpenTime = new TimeSpan(6, 0, 0),
                        ClosingTime = new TimeSpan(23, 0, 0),
                        IsClosed = false
                    });
                }

                // Saturday: 08:00 - 20:00
                openingHours.Add(new GymOpeningHour
                {
                    GymId = gym.Id,
                    DayOfWeek = DayOfWeek.Saturday,
                    OpenTime = new TimeSpan(8, 0, 0),
                    ClosingTime = new TimeSpan(20, 0, 0),
                    IsClosed = false
                });

                // Sunday: 10:00 - 18:00
                openingHours.Add(new GymOpeningHour
                {
                    GymId = gym.Id,
                    DayOfWeek = DayOfWeek.Sunday,
                    OpenTime = new TimeSpan(10, 0, 0),
                    ClosingTime = new TimeSpan(18, 0, 0),
                    IsClosed = false
                });
            }

            await context.GymOpeningHours.AddRangeAsync(openingHours);
            await context.SaveChangesAsync();
        }

                // 9. Seed Trainer Schedules
                if (!await context.TrainerSchedules.AnyAsync())
                {
                    var trainers = await context.Trainers.ToListAsync();
                    var schedules = new List<TrainerSchedule>();

                    foreach (var trainer in trainers)
                    {
                        // Monday - Friday: 09:00 - 18:00
                        for (var day = DayOfWeek.Monday; day <= DayOfWeek.Friday; day++)
                        {
                            schedules.Add(new TrainerSchedule(
                                trainer.Id,
                                day,
                                new TimeSpan(9, 0, 0),
                                new TimeSpan(18, 0, 0)
                            ));
                        }

                        // Saturday: 10:00 - 15:00
                        schedules.Add(new TrainerSchedule(
                            trainer.Id,
                            DayOfWeek.Saturday,
                            new TimeSpan(10, 0, 0),
                            new TimeSpan(15, 0, 0)
                        ));
                    }

                    await context.TrainerSchedules.AddRangeAsync(schedules);
                    await context.SaveChangesAsync();
                }
            }
        }
