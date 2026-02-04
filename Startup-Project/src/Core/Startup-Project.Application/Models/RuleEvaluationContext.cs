namespace Startup_Project.Application.Models;

public class RuleEvaluationContext
{
    public Guid UserId { get; set; }
    public Guid PackageId { get; set; }
    public decimal OrderAmount { get; set; }
    public DateTime CurrentDateTime { get; set; } = DateTime.UtcNow;
    public string? UserSegment { get; set; }
    public bool IsNewUser { get; set; }
    public int TotalPurchaseCount { get; set; }
}
