
using Startup_Project.Domain.Entities.Common;

namespace Startup_Project.Domain.Entities;

public class TrainerSchedule :BaseEntity
{
    public Guid TrainerId { get; private set; }
    public Trainer Trainer { get; private set; } = null!;

    public DayOfWeek DayOfWeek { get; private set; }
    public TimeSpan StartTime { get; private set; }
    public TimeSpan EndTime { get; private set; }
    public bool IsAvailable { get; private set; } = true;
    private TrainerSchedule() { }

    public TrainerSchedule(Guid trainerId, DayOfWeek dayOfWeek, TimeSpan startTime, TimeSpan endTime)
    {
        if (endTime <= startTime)
            throw new ArgumentException("Bitis vaxti baslangic vaxtindan boyuk olmalidir.");

        TrainerId = trainerId;
        DayOfWeek = dayOfWeek;
        StartTime = startTime;
        EndTime = endTime;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(TimeSpan startTime, TimeSpan endTime, bool isAvailable)
    {
        if (endTime <= startTime)
            throw new ArgumentException("Bitis vaxti baslangic vaxtindan boyuk olmalidir.");

        StartTime = startTime;
        EndTime = endTime;
        IsAvailable = isAvailable;
        UpdatedAt = DateTime.UtcNow;
    }
}
