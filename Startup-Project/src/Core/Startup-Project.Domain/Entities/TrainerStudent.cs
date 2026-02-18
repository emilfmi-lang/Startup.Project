using Startup_Project.Domain.Entities.Common;
using Startup_Project.Domain.Entities.Identity;

namespace Startup_Project.Domain.Entities;

public class TrainerStudent : BaseEntity
{
    public Guid TrainerId { get; private set; }
    public Trainer Trainer { get; private set; } = null!;

    public Guid StudentId { get; private set; }
    public AppUser Student { get; private set; } = null!;

    public string Program { get; private set; } = null!;
    public int TotalSessions { get; private set; }

    private TrainerStudent() { }

    public TrainerStudent(Guid trainerId, Guid studentId, string program, int totalSessions)
    {
        TrainerId = trainerId;
        StudentId = studentId;
        Program = program;
        TotalSessions = totalSessions > 0 ? totalSessions : 1;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateProgram(string program)
    {
        if (!string.IsNullOrWhiteSpace(program))
        {
            Program = program;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void UpdateTotalSessions(int totalSessions)
    {
        if (totalSessions > 0)
        {
            TotalSessions = totalSessions;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
