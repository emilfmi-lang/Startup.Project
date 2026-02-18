using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Startup_Project.Application.Interfaces;
using Startup_Project.Domain.Entities;
using Startup_Project.Domain.Entities.Identity;

namespace Startup_Project.Persistance.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>(options), IApplicationDbContext
{
    public DbSet<Gym> Gyms { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<Package> Packages { get; set; }
    public DbSet<GymReview> GymReviews { get; set; }
    public DbSet<GymOpeningHour> GymOpeningHours { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<CheckInLog> CheckInLogs { get; set; }
    public DbSet<CheckInToken> CheckInTokens { get; set; }
    public DbSet<Trainer> Trainers { get; set; }
    public DbSet<TrainerReview> TrainerReviews { get; set; }
    public DbSet<TrainerSchedule> TrainerSchedules { get; set; }
    public DbSet<TrainingSession> TrainingSessions { get; set; }
    public DbSet<SmsOtp> SmsOtps { get; set; }
    public DbSet<EmailOtp> EmailOtps { get; set; }
    public DbSet<FavoriteGym> FavoriteGyms { get; set; }
    public DbSet<PromoCode> PromoCodes { get; set; }
    public DbSet<PromoCodeUsage> PromoCodeUsages { get; set; }
    public DbSet<Campaign> Campaigns { get; set; }
    public DbSet<CampaignRule> CampaignRules { get; set; }
    public DbSet<CampaignUsage> CampaignUsages { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<UserDevice> UserDevices { get; set; }
    public DbSet<GymAmenity> GymAmenities { get; set; }
    public DbSet<GymImage> GymImages { get; set; }
    public DbSet<PackageFeature> PackageFeatures { get; set; }
    public DbSet<FavoriteTrainer> FavoriteTrainers { get; set; }
    public DbSet<UserSession> UserSessions { get; set; }
    public DbSet<UserPaymentMethod> UserPaymentMethods { get; set; }
    public DbSet<ScheduledSession> ScheduledSessions { get; set; }
    public DbSet<TrainerStudent> TrainerStudents { get; set; }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(cancellationToken);
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
