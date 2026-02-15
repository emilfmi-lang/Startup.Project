
using Microsoft.EntityFrameworkCore;
using Startup_Project.Domain.Entities;

namespace Startup_Project.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Gym> Gyms { get; set; }
    DbSet<Subscription> Subscriptions { get; set; }
    DbSet<Package> Packages { get; set; }
    DbSet<GymReview> GymReviews { get; set; }
    DbSet<GymOpeningHour> GymOpeningHours { get; set; }
    DbSet<Payment> Payments { get; set; }
    DbSet<CheckInLog> CheckInLogs { get; set; }
    DbSet<CheckInToken> CheckInTokens { get; set; }
    DbSet<Trainer> Trainers { get; set; }
    DbSet<TrainerReview> TrainerReviews { get; set; }
    DbSet<TrainerSchedule> TrainerSchedules { get; set; }
    DbSet<TrainingSession> TrainingSessions { get; set; }
    DbSet<SmsOtp> SmsOtps { get; set; }
    DbSet<FavoriteGym> FavoriteGyms { get; set; }
    DbSet<PromoCode> PromoCodes { get; set; }
    DbSet<PromoCodeUsage> PromoCodeUsages { get; set; }
    DbSet<Campaign> Campaigns { get; set; }
    DbSet<CampaignRule> CampaignRules { get; set; }
    DbSet<CampaignUsage> CampaignUsages { get; set; }
    DbSet<Notification> Notifications { get; set; }
    DbSet<UserDevice> UserDevices { get; set; }
    DbSet<GymAmenity> GymAmenities { get; set; }
    DbSet<GymImage> GymImages { get; set; }
    DbSet<PackageFeature> PackageFeatures { get; set; }
    DbSet<FavoriteTrainer> FavoriteTrainers { get; set; }
    DbSet<ScheduledSession> ScheduledSessions { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
