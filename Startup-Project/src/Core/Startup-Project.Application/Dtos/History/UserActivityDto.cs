using Startup_Project.Application.Enums;

namespace Startup_Project.Application.Dtos.History;

public record UserActivityDto
{
    public Guid Id { get; init; }
    public ActivityType ActivityType { get; init; }
    public string Title { get; init; } = null!;
    public string? Description { get; init; }
    public decimal? Amount { get; init; }
    public ActivityStatus Status { get; init; }
    public string StatusText { get; init; } = null!;
    public DateTime Date { get; init; }
    public string Icon { get; init; } = null!;
    public string Color { get; init; } = null!;
    public string? GymName { get; init; }
    public string? PackageName { get; init; }
}
