namespace Startup_Project.Application.Dtos.History;

/// <summary>
/// Gün üzr? qrupla?d?r?lm?? aktivlikl?r
/// UI: "Saturday, Jan 20 - 2 activities"
/// </summary>
public class ActivityTimelineDto
{
    /// <summary>
    /// Gün tarixi (saat 00:00:00)
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Gün label: "Saturday, Jan 20"
    /// </summary>
    public string DateLabel { get; set; } = null!;

    /// <summary>
    /// Günd?ki aktivlik say?
    /// </summary>
    public int ActivitiesCount { get; set; }

    /// <summary>
    /// Aktivlik say? label: "2 activities" | "1 activity"
    /// </summary>
    public string ActivitiesCountLabel { get; set; } = null!;

    /// <summary>
    /// O günd?ki aktivlik tipl?ri: ["gym", "trainer"]
    /// </summary>
    public List<string> ActivityTypes { get; set; } = [];

    /// <summary>
    /// O günd?ki aktivlikl?r siyah?s?
    /// </summary>
    public List<TimelineActivityDto> Activities { get; set; } = [];
}
