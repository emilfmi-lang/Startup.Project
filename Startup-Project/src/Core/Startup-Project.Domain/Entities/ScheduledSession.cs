using Microsoft.EntityFrameworkCore.Query;
using Startup_Project.Domain.Entities.Common;
using Startup_Project.Domain.Entities.Identity;
using Startup_Project.Domain.Enums;

namespace Startup_Project.Domain.Entities;

public class ScheduledSession : BaseEntity
{
    public Guid TrainerId { get; private set; }
    public Trainer Trainer { get; private set; } = null!;

    public Guid? MemberId { get; private set; }
    public AppUser? Member { get; private set; }

    public Guid GymId { get; private set; }
    public Gym Gym { get; private set; } = null!;

    public string? Title { get; private set; }
    public string ExerciseType { get; private set; } = null!;
    public string? Description { get; private set; }
    public DateTime ScheduledTime { get; private set; }
    public int DurationMinutes { get; private set; }
    public int MaxParticipants { get; private set; } = 1;
    public decimal PricePerPerson { get; private set; }
    public ScheduledSessionStatus Status { get; private set; } = ScheduledSessionStatus.Upcoming;

    private ScheduledSession() { }

    public ScheduledSession(
        Guid trainerId,
        Guid memberId,
        Guid gymId,
        string exerciseType,
        DateTime scheduledTime,
        int durationMinutes)
    {
        TrainerId = trainerId;
        MemberId = memberId;
        GymId = gymId;
        ExerciseType = exerciseType;
        ScheduledTime = scheduledTime;
        DurationMinutes = durationMinutes;
        MaxParticipants = 1;
        Status = ScheduledSessionStatus.Upcoming;
        CreatedAt = DateTime.UtcNow;
    }

    public ScheduledSession(
        Guid trainerId,
        Guid gymId,
        string title,
        string exerciseType,
        string? description,
        DateTime scheduledTime,
        int durationMinutes,
        int maxParticipants,
        decimal pricePerPerson)
    {
        TrainerId = trainerId;
        GymId = gymId;
        Title = title;
        ExerciseType = exerciseType;
        Description = description;
        ScheduledTime = scheduledTime;
        DurationMinutes = durationMinutes;
        MaxParticipants = maxParticipants;
        PricePerPerson = pricePerPerson;
        Status = ScheduledSessionStatus.Upcoming;
        CreatedAt = DateTime.UtcNow;
    }

    public DateTime EndTime => ScheduledTime.AddMinutes(DurationMinutes);

    public void MarkAsCompleted()
    {
        if (Status != ScheduledSessionStatus.Upcoming)
            throw new InvalidOperationException("Yalniz gozleyen seans tamamlana biler.");

        Status = ScheduledSessionStatus.Completed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status != ScheduledSessionStatus.Upcoming)
            throw new InvalidOperationException("Yalniz gozleyen seans legv edile biler.");

        Status = ScheduledSessionStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool OverlapsWith(DateTime otherStart, int otherDurationMinutes)
    {
        var otherEnd = otherStart.AddMinutes(otherDurationMinutes);
        return ScheduledTime < otherEnd && EndTime > otherStart;
    }
}
