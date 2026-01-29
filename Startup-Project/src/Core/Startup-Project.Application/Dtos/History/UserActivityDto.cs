using Startup_Project.Application.Enums;

namespace Startup_Project.Application.Dtos.History;

public class UserActivityDto
{
    public Guid Id { get; set; }
    public ActivityType ActivityType { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public decimal? Amount { get; set; }
    public ActivityStatus Status { get; set; }
    public string StatusText { get; set; } = null!;
    public DateTime Date { get; set; }
    public string Icon { get; set; } = null!;
    public string Color { get; set; } = null!;
    public string? GymName { get; set; }
    public string? PackageName { get; set; }
}
