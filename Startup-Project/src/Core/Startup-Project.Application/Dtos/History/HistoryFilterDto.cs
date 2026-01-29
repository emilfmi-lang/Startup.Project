using Startup_Project.Application.Enums;

namespace Startup_Project.Application.Dtos.History;

public record HistoryFilterDto
{
    public ActivityStatus? Status { get; init; }
    public ActivityType? ActivityType { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
