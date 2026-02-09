namespace Startup_Project.Application.Dtos.History;

/// <summary>
/// History s?hif?sinin tam response-u
/// Bütün bölm?l?ri ?hat? edir: Stats, Weekly Chart, Timeline, Achievements
/// </summary>
public class FullHistoryResponseDto
{
    /// <summary>
    /// Yuxar?dak? 4 statistika kart?
    /// </summary>
    public ActivityStatsDto Stats { get; set; } = null!;

    /// <summary>
    /// Weekly Activity chart data (7 gün)
    /// </summary>
    public List<WeeklyActivityDto> WeeklyActivity { get; set; } = [];

    /// <summary>
    /// Gün üzr? qrupla?d?r?lm?? aktivlikl?r timeline-?
    /// </summary>
    public List<ActivityTimelineDto> Timeline { get; set; } = [];

    /// <summary>
    /// Monthly Achievements (3 kart)
    /// </summary>
    public List<MonthlyAchievementDto> Achievements { get; set; } = [];

    /// <summary>
    /// Seçilmi? period: "This Week" | "This Month" | "This Year"
    /// </summary>
    public string Period { get; set; } = null!;

    /// <summary>
    /// Period ba?lan??c tarixi
    /// </summary>
    public DateTime PeriodStart { get; set; }

    /// <summary>
    /// Period biti? tarixi
    /// </summary>
    public DateTime PeriodEnd { get; set; }
}
