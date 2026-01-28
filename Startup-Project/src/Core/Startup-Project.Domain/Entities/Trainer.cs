using Startup_Project.Domain.Entities.Common;
using Startup_Project.Domain.Entities.Identity;
using Startup_Project.Domain.Enums;

namespace Startup_Project.Domain.Entities;

public class Trainer : BaseEntity
{
    public Guid UserId { get; private set; }
    public AppUser User { get; private set; } = null!;
    public Guid GymId { get; private set; }
    public Gym Gym { get; private set; } = null!;
    public string Specialization { get; private set; } = null!;
    public string? Bio { get; private set; }
    public string? CertificateUrl { get; private set; }
    public string? ProfileImageUrl { get; private set; }
    public double AverageRating { get; private set; } = 0;
    public int ReviewCount { get; private set; } = 0;
    public int TotalClients { get; private set; } = 0;
    public int TotalSessions { get; private set; } = 0;
    public TrainerStatus Status { get; private set; } = TrainerStatus.Pending;
    public List<TrainerReview> Reviews { get; private set; } = [];
    public List<TrainerSchedule> Schedules { get; private set; } = [];
    public decimal SessionPrice { get; private set; }

    public List<TrainingSession> Sessions { get; private set; } = [];
    private Trainer() { }
    public void SetSessionPrice(decimal price)
    {
        if (price < 0)
            throw new ArgumentException("Qiymet menfi ola bilmez.");
        SessionPrice = price;
        UpdatedAt = DateTime.UtcNow;
    }
    public Trainer(Guid userId, Guid gymId, string specialization, decimal sessionPrice)
    {
        UserId = userId;
        GymId = gymId;
        SetSpecialization(specialization);
        SetSessionPrice(sessionPrice);
        Status = TrainerStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    // Methods
    public void SetSpecialization(string specialization)
    {
        if (string.IsNullOrWhiteSpace(specialization))
            throw new ArgumentException("Specialization bos ola bilmez.");
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
