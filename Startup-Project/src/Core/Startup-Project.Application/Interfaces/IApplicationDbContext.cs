
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
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
