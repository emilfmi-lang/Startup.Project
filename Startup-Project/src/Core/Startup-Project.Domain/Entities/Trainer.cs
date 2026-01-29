using Startup_Project.Domain.Entities.Common;
using Startup_Project.Domain.Entities.Identity;
using Startup_Project.Domain.Enums;

namespace Startup_Project.Domain.Entities;

public class Trainer : BaseEntity
{
    public Guid UserId { get; private set; }
    public AppUser User { get; private set; } = null!;

    // GymId nullable - Online trainer ucun NULL olacaq
    public Guid? GymId { get; private set; }
    public Gym? Gym { get; private set; }

    public string Specialization { get; private set; } = null!;
    public string? Bio { get; private set; }
    public string? CertificateUrl { get; private set; }
    public string? ProfileImageUrl { get; private set; }

    // Hybrid Trainer - Online/Gym-based
    public bool IsOnlineTrainer { get; private set; } = false;
    public bool IsVerified { get; private set; } = false;
    public string? VideoIntroUrl { get; private set; }
    public string? SocialMediaLinks { get; private set; }  // JSON format

    // Statistics
    public double AverageRating { get; private set; } = 0;
    public int ReviewCount { get; private set; } = 0;
    public int TotalClients { get; private set; } = 0;
    public int TotalSessions { get; private set; } = 0;

    // Price & Status
    public decimal SessionPrice { get; private set; }
    public TrainerStatus Status { get; private set; } = TrainerStatus.Pending;

    // Navigation Properties
    public List<TrainerReview> Reviews { get; private set; } = [];
    public List<TrainerSchedule> Schedules { get; private set; } = [];
    public List<TrainingSession> Sessions { get; private set; } = [];

    // Constructors
    private Trainer() { }

    // Gym-based Trainer Constructor
    public Trainer(Guid userId, Guid gymId, string specialization, decimal sessionPrice)
    {
        UserId = userId;
        GymId = gymId;
        IsOnlineTrainer = false;
        SetSpecialization(specialization);
        SetSessionPrice(sessionPrice);
        Status = TrainerStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    // Online Trainer Constructor
    public Trainer(Guid userId, string specialization, decimal sessionPrice)
    {
        UserId = userId;
        GymId = null;
        IsOnlineTrainer = true;
        IsVerified = false;
        SetSpecialization(specialization);
        SetSessionPrice(sessionPrice);
        Status = TrainerStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    // Methods
    public void SetSessionPrice(decimal price)
    {
        if (price < 0)
            throw new ArgumentException("Qiymet menfi ola bilmez.");
        SessionPrice = price;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetSpecialization(string specialization)
    {
        if (string.IsNullOrWhiteSpace(specialization))
            throw new ArgumentException("Ixtisas bos ola bilmez.");
        Specialization = specialization;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetBio(string? bio)
    {
        Bio = bio;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetCertificateUrl(string? url)
    {
        CertificateUrl = url;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetProfileImageUrl(string? url)
    {
        ProfileImageUrl = url;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetVideoIntroUrl(string? url)
    {
        VideoIntroUrl = url;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetSocialMediaLinks(string? json)
    {
        SocialMediaLinks = json;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        Status = TrainerStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        Status = TrainerStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Suspend()
    {
        Status = TrainerStatus.Suspended;
        UpdatedAt = DateTime.UtcNow;
    }

    // Online Trainer Verification
    public void Verify()
    {
        if (!IsOnlineTrainer)
            throw new InvalidOperationException("Yalniz online trainer verify edile biler.");
        IsVerified = true;
        Status = TrainerStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reject()
    {
        Status = TrainerStatus.Rejected;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddReview(int starCount)
    {
        var total = AverageRating * ReviewCount;
        ReviewCount++;
        AverageRating = (total + starCount) / ReviewCount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveReview(int starCount)
    {
        if (ReviewCount <= 0) return;

        var total = AverageRating * ReviewCount;
        ReviewCount--;
        AverageRating = ReviewCount == 0 ? 0 : (total - starCount) / ReviewCount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void IncrementClients()
    {
        TotalClients++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void IncrementSessions()
    {
        TotalSessions++;
        UpdatedAt = DateTime.UtcNow;
    }
}
