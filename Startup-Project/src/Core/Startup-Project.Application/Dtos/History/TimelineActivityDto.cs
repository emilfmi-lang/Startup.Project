namespace Startup_Project.Application.Dtos.History;

/// <summary>
/// Activity Timeline-dak? t?k aktivlik
/// UI: "Gym Check-in - FitLife Premium Gym • 09:30 • 90 min"
/// </summary>
public class TimelineActivityDto
{
    /// <summary>
    /// Aktivlik ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Aktivlik tipi: "gym-checkin" | "training-session"
    /// </summary>
    public string Type { get; set; } = null!;

    /// <summary>
    /// Aktivlik ba?l???: "Gym Check-in" | "Strength Training Session"
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// Alt ba?l?q: "FitLife Premium Gym • 09:30 • 90 min"
    /// </summary>
    public string Subtitle { get; set; } = null!;

    /// <summary>
    /// Gym ad?
    /// </summary>
    public string? GymName { get; set; }

    /// <summary>
    /// Trainer ad?
    /// </summary>
    public string? TrainerName { get; set; }

    /// <summary>
    /// Saat format?nda: "09:30"
    /// </summary>
    public string Time { get; set; } = null!;

    /// <summary>
    /// Müdd?t d?qiq? il?
    /// </summary>
    public int Duration { get; set; }

    /// <summary>
    /// Müdd?t label: "90 min"
    /// </summary>
    public string DurationLabel { get; set; } = null!;

    /// <summary>
    /// Icon tipi: "gym" | "trainer"
    /// </summary>
    public string Icon { get; set; } = null!;
}
