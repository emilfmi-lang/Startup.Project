namespace Startup_Project.Application.Dtos.History;

public record HistorySummaryDto
{
    public int TotalPayments { get; init; }
    public int TotalCheckIns { get; init; }
    public decimal TotalSpent { get; init; }
    public int SuccessfulCount { get; init; }
    public int PendingCount { get; init; }
    public int FailedCount { get; init; }
}
