namespace Startup_Project.Application.Dtos.Trainer;

public class TrainerDashboardDto
{
    public TrainerDashboardStatsDto Stats { get; set; } = null!;
    public List<UpcomingSessionDto> UpcomingSessions { get; set; } = [];
    public List<TopStudentDto> TopStudents { get; set; } = [];
}

public class TrainerDashboardStatsDto
{
    public int ActiveStudents { get; set; }
    public int NewStudentsThisMonth { get; set; }

    public int SessionsThisWeek { get; set; }
    public int RemainingSessionsThisWeek { get; set; }

    public decimal MonthlyEarnings { get; set; }
    public decimal EarningsChangePercent { get; set; }

    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
}

public class UpcomingSessionDto
{
    public Guid Id { get; set; }
    public string StudentName { get; set; } = null!;
    public string? StudentInitial { get; set; }
    public string ExerciseType { get; set; } = null!;
    public string? Title { get; set; }
    public string StartTime { get; set; } = null!;
    public string EndTime { get; set; } = null!;
    public string DateLabel { get; set; } = null!;
    public string Status { get; set; } = null!;
}

public class TopStudentDto
{
    public Guid StudentId { get; set; }
    public string FullName { get; set; } = null!;
    public string? Initial { get; set; }
    public int TotalSessions { get; set; }
    public int CompletedSessions { get; set; }
    public double AttendancePercent { get; set; }
}
