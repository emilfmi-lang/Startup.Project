namespace Startup_Project.Application.Dtos.History;

public record HistoryResponseDto
{
    public List<UserActivityDto> Activities { get; init; } = [];
    public int TotalCount { get; init; }
    public int CurrentPage { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
    public bool HasNextPage => CurrentPage < TotalPages;
    public bool HasPreviousPage => CurrentPage > 1;
    public HistorySummaryDto Summary { get; init; } = null!;
}
