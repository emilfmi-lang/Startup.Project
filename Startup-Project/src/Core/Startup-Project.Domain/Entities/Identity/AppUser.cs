using Microsoft.AspNetCore.Identity;
using Startup_Project.Domain.Enums;

namespace Startup_Project.Domain.Entities.Identity;

public class AppUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid? GymId { get; set; }
    public List<Gym> Gyms { get; set; } = [];
    public string? VerificationCode { get; set; }
    public DateTime? CodeExpireDate { get; set; } 
    public List<GymReview> Reviews { get; set; } = [];
    public string? GoogleId { get; set; }
    public AuthProvider AuthProvider { get; set; } = AuthProvider.Local;
    public Trainer? TrainerProfile { get; set; }
    public List<TrainerReview> TrainerReviews { get; set; } = [];
    public List<TrainingSession> TrainingSessions { get; set; } = [];
    public List<ScheduledSession> ScheduledSessions { get; set; } = [];
    public List<FavoriteGym> FavoriteGyms { get; private set; } = [];
    public List<Notification> Notifications { get; private set; } = [];
    public List<UserDevice> Devices { get; private set; } = [];
    public List<FavoriteTrainer> FavoriteTrainers { get; private set; } = [];
    public string? Bio { get; set; }
    public string? ProfileImagePath { get; set; }
    public string? ProfileImageUrl { get; set; }
    public bool NotifyPush { get; set; } = true;
    public bool NotifyEmail { get; set; } = true;
    public bool NotifySms { get; set; } = false;
    public bool NotifyWorkoutReminders { get; set; } = true;
    public bool NotifyPromotions { get; set; } = true;
    public bool NotifySessionUpdates { get; set; } = true;
    public bool NotifyNewFeatures { get; set; } = false;
    public string Language { get; set; } = "az";
    public string Theme { get; set; } = "dark";
    public string DistanceUnit { get; set; } = "km";
    public string TimeZone { get; set; } = "Asia/Baku";
    public List<UserSession> Sessions { get; private set; } = [];
    public List<UserPaymentMethod> PaymentMethods { get; private set; } = [];
    public List<ConversationParticipant> ChatParticipations { get; private set; } = [];
    public List<ChatMessage> SentMessages { get; private set; } = [];

    // Soft Delete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletionReason { get; set; }

    public AppUser() { }

    public AppUser(string fullName)
    {
        FullName = fullName;
    }

    public AppUser(string fullName, string googleId, AuthProvider authProvider)
    {
        FullName = fullName;
        GoogleId = googleId;
        AuthProvider = authProvider;
    }
}
