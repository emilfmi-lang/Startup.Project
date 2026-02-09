namespace Startup_Project.Application.Dtos.History;

/// <summary>
/// Weekly Activity chart üçün - h?ft?nin h?r günü üçün data
/// UI: Bar chart - Mon, Tue, Wed, Thu, Fri, Sat, Sun
/// </summary>
public class WeeklyActivityDto
{
    /// <summary>
    /// H?ft?nin günü q?sa ad?: "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"
    /// </summary>
    public string DayOfWeek { get; set; } = null!;

    /// <summary>
    /// Tam tarix
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// O gün m??q edilmi? ümumi d?qiq?l?r
    /// </summary>
    public int WorkoutMinutes { get; set; }

    /// <summary>
    /// Saat olaraq (WorkoutMinutes / 60)
    /// </summary>
    public double WorkoutHours { get; set; }

    /// <summary>
    /// O gün h?r hans? aktivlik varm??
    /// </summary>
    public bool HasActivity { get; set; }
}
