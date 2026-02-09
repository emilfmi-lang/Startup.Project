namespace Startup_Project.Application.Dtos.History;

/// <summary>
/// Activity History s?hif?sinin yuxar?s?ndak? 4 statistika kart?
/// UI: Gym Visits (18), Hours Trained (27h), PT Sessions (6), Day Streak (5)
/// </summary>
public class ActivityStatsDto
{
    /// <summary>
    /// Gym-? giri? say?
    /// </summary>
    public int GymVisits { get; set; }

    /// <summary>
    /// Label: "Gym Visits"
    /// </summary>
    public string GymVisitsLabel { get; set; } = "Gym Visits";

    /// <summary>
    /// Ümumi m??q saatlar?
    /// </summary>
    public double HoursTrained { get; set; }

    /// <summary>
    /// Label: "27h" format?nda
    /// </summary>
    public string HoursTrainedLabel { get; set; } = null!;

    /// <summary>
    /// Personal Trainer sessiyalar? say?
    /// </summary>
    public int PtSessions { get; set; }

    /// <summary>
    /// Label: "PT Sessions"
    /// </summary>
    public string PtSessionsLabel { get; set; } = "PT Sessions";

    /// <summary>
    /// Ard?c?l giri? günl?ri say?
    /// </summary>
    public int DayStreak { get; set; }

    /// <summary>
    /// Label: "Day Streak"
    /// </summary>
    public string DayStreakLabel { get; set; } = "Day Streak";
}
