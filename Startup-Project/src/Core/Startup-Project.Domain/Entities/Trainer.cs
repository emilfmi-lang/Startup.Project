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
    private Trainer() { }

   

}
