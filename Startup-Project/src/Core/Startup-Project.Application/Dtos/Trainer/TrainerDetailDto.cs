using Startup_Project.Application.Dtos.TrainerReview;
using Startup_Project.Application.Dtos.TrainerSchedule;

namespace Startup_Project.Application.Dtos.Trainer;

public class TrainerDetailDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserFullName { get; set; } = null!;
    public string? ProfileImageUrl { get; set; }
    public bool IsVerified { get; set; }
    public bool IsOnlineTrainer { get; set; }
    public string Status { get; set; } = null!;

    // Gym info (Works At sidebar)
    public Guid? GymId { get; set; }
    public string? GymName { get; set; }
    public string? GymAddress { get; set; }
    public string? GymImagePath { get; set; }

    // Stats cards
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public int TotalClients { get; set; }
    public int TotalSessions { get; set; }
    public decimal SessionPrice { get; set; }
    public int YearsExperience { get; set; }

    // About section
    public string? Bio { get; set; }
    public string Specialization { get; set; } = null!;
    public List<string> Specializations { get; set; } = [];

    // Online trainer fields
    public string? VideoIntroUrl { get; set; }
    public string? SocialMediaLinks { get; set; }
    public string? CertificateUrl { get; set; }

    // Favorite
    public bool IsFavorite { get; set; }
    public int FavoriteCount { get; set; }

    // Tabs data
    public List<TrainerReviewDto> Reviews { get; set; } = [];
    public List<TrainerScheduleDto> Schedule { get; set; } = [];
}
