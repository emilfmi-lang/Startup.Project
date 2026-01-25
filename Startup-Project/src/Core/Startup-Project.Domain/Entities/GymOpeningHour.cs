using Startup_Project.Domain.Entities.Common;

namespace Startup_Project.Domain.Entities;

public class GymOpeningHour : BaseEntity
{
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan OpenTime { get; set; }
    public TimeOnly ClosingTime { get; set; }
    public bool IsClosed { get; set; }
    public Guid GymId { get; set; }
    public Gym Gym { get; set; } = null!;
}
