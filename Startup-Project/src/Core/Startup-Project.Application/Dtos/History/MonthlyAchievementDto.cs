namespace Startup_Project.Application.Dtos.History;

/// <summary>
/// Monthly Achievements bölm?sind?ki progress kartlar?
/// UI: "Consistency Goal 18/20" progress bar il?
/// </summary>
public class MonthlyAchievementDto
{
    /// <summary>
    /// Achievement tipi: "consistency" | "hours" | "sessions"
    /// </summary>
    public string Type { get; set; } = null!;

    /// <summary>
    /// Ba?l?q: "Consistency Goal" | "Training Hours" | "PT Sessions"
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// Cari d?y?r (?ld? edilmi?)
    /// </summary>
    public int Current { get; set; }

    /// <summary>
    /// H?d?f d?y?r
    /// </summary>
    public int Target { get; set; }

    /// <summary>
    /// Progress label: "18/20"
    /// </summary>
    public string ProgressLabel { get; set; } = null!;

    /// <summary>
    /// Progress faiz: 90.0 (0-100 aras?)
    /// </summary>
    public double ProgressPercent { get; set; }

    /// <summary>
    /// Icon ad?: "target" | "clock" | "user"
    /// </summary>
    public string Icon { get; set; } = null!;
}
