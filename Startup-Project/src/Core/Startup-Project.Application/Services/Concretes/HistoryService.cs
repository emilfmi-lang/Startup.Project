using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Startup_Project.Application.Dtos.History;
using Startup_Project.Application.Enums;
using Startup_Project.Application.Models;
using Startup_Project.Application.Services.Interfaces;
using Startup_Project.Domain.Abstractions.Repositories;
using Startup_Project.Domain.Entities;
using Startup_Project.Domain.Enums;
using Startup_Project.Domain.Exceptions;
using System.Security.Claims;

namespace Startup_Project.Application.Services.Concretes;

public class HistoryService(
    IGenericRepository<Payment> paymentRepository,
    IGenericRepository<CheckInLog> checkInRepository,
    IGenericRepository<TrainingSession> sessionRepository,
    IMapper mapper,
    IHttpContextAccessor httpContextAccessor) : IHistoryService
{
    #region -------------------- PUBLIC METHODS --------------------

    public async Task<ResponseModel<FullHistoryResponseDto>> GetFullHistoryAsync(string period = "week")
    {
        var userId = GetCurrentUserId();
        var (startDate, endDate, periodLabel) = GetPeriodDates(period);

        var stats = await CalculateStatsAsync(userId, startDate, endDate);
        var weeklyActivity = await CalculateWeeklyActivityAsync(userId);
        var timeline = await BuildTimelineAsync(userId, startDate, endDate);
        var achievements = BuildAchievements(stats);

        return ResponseModel<FullHistoryResponseDto>.SuccessResponse(new FullHistoryResponseDto
        {
            Stats = stats,
            WeeklyActivity = weeklyActivity,
            Timeline = timeline,
            Achievements = achievements,
            Period = periodLabel,
            PeriodStart = startDate,
            PeriodEnd = endDate
        });
    }

    public async Task<ResponseModel<List<ActivityTimelineDto>>> GetActivityTimelineAsync(
        DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 10)
    {
        var userId = GetCurrentUserId();
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        var timeline = await BuildTimelineAsync(userId, start, end);
        var paginated = timeline.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return ResponseModel<List<ActivityTimelineDto>>.SuccessResponse(paginated);
    }

    public async Task<ResponseModel<ActivityStatsDto>> GetActivityStatsAsync(string period = "month")
    {
        var userId = GetCurrentUserId();
        var (startDate, endDate, _) = GetPeriodDates(period);
        var stats = await CalculateStatsAsync(userId, startDate, endDate);

        return ResponseModel<ActivityStatsDto>.SuccessResponse(stats);
    }

    public async Task<ResponseModel<List<WeeklyActivityDto>>> GetWeeklyActivityAsync()
    {
        var userId = GetCurrentUserId();
        var weeklyActivity = await CalculateWeeklyActivityAsync(userId);

        return ResponseModel<List<WeeklyActivityDto>>.SuccessResponse(weeklyActivity);
    }

    public async Task<ResponseModel<List<MonthlyAchievementDto>>> GetMonthlyAchievementsAsync()
    {
        var userId = GetCurrentUserId();
        var (startDate, endDate, _) = GetPeriodDates("month");
        var stats = await CalculateStatsAsync(userId, startDate, endDate);

        return ResponseModel<List<MonthlyAchievementDto>>.SuccessResponse(BuildAchievements(stats));
    }

    public async Task<ResponseModel<HistoryResponseDto>> GetMyHistoryAsync(HistoryFilterDto filter)
    {
        var userId = GetCurrentUserId();
        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);

        var activities = await GetUnifiedActivitiesAsync(userId, filter);
        var summary = await GetSummaryForUserAsync(userId);

        return ResponseModel<HistoryResponseDto>.SuccessResponse(new HistoryResponseDto
        {
            Activities = activities.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
            TotalCount = activities.Count,
            CurrentPage = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(activities.Count / (double)pageSize),
            Summary = summary
        });
    }

    public async Task<ResponseModel<HistorySummaryDto>> GetHistorySummaryAsync()
    {
        var userId = GetCurrentUserId();
        return ResponseModel<HistorySummaryDto>.SuccessResponse(await GetSummaryForUserAsync(userId));
    }

    public async Task<ResponseModel<List<UserActivityDto>>> GetRecentActivitiesAsync(int count = 10)
    {
        var userId = GetCurrentUserId();
        var activities = await GetUnifiedActivitiesAsync(userId, new HistoryFilterDto());

        return ResponseModel<List<UserActivityDto>>.SuccessResponse(
            activities.Take(Math.Clamp(count, 1, 50)).ToList());
    }

    #endregion

    #region -------------------- STATS --------------------

    private async Task<ActivityStatsDto> CalculateStatsAsync(Guid userId, DateTime startDate, DateTime endDate)
    {
        var gymVisits = await checkInRepository.Table
            .CountAsync(c => c.UserId == userId
                && c.CheckInTime >= startDate && c.CheckInTime <= endDate
                && c.Status == CheckInStatus.Success);

        var hoursTrained = await CalculateHoursTrainedAsync(userId, startDate, endDate);
        var ptSessions = await sessionRepository.Table
            .CountAsync(s => s.UserId == userId
                && s.UsedAt >= startDate && s.UsedAt <= endDate
                && s.Status == SessionStatus.Used);

        var dayStreak = await CalculateDayStreakAsync(userId);

        return new ActivityStatsDto
        {
            GymVisits = gymVisits,
            GymVisitsLabel = "Gym Visits",
            HoursTrained = hoursTrained,
            HoursTrainedLabel = $"{hoursTrained}h",
            PtSessions = ptSessions,
            PtSessionsLabel = "PT Sessions",
            DayStreak = dayStreak,
            DayStreakLabel = "Day Streak"
        };
    }

    private async Task<double> CalculateHoursTrainedAsync(Guid userId, DateTime startDate, DateTime endDate)
    {
        var checkInsWithCheckout = await checkInRepository.Table
            .Where(c => c.UserId == userId
                && c.CheckInTime >= startDate && c.CheckInTime <= endDate
                && c.CheckOutTime != null)
            .ToListAsync();

        var totalMinutes = checkInsWithCheckout.Sum(c => (c.CheckOutTime!.Value - c.CheckInTime).TotalMinutes);

        var checkInsWithoutCheckout = await checkInRepository.Table
            .CountAsync(c => c.UserId == userId
                && c.CheckInTime >= startDate && c.CheckInTime <= endDate
                && c.CheckOutTime == null && c.Status == CheckInStatus.Success);

        totalMinutes += checkInsWithoutCheckout * 60;
        return Math.Round(totalMinutes / 60, 1);
    }

    private async Task<int> CalculateDayStreakAsync(Guid userId)
    {
        var dates = await checkInRepository.Table
            .Where(c => c.UserId == userId && c.Status == CheckInStatus.Success)
            .Select(c => c.CheckInTime.Date)
            .Distinct()
            .OrderByDescending(d => d)
            .Take(60)
            .ToListAsync();

        if (dates.Count == 0) return 0;

        var streak = 0;
        var expected = DateTime.UtcNow.Date;

        foreach (var date in dates)
        {
            if (date == expected || date == expected.AddDays(-1))
            {
                streak++;
                expected = date.AddDays(-1);
            }
            else break;
        }

        return streak;
    }

    #endregion

    #region -------------------- WEEKLY ACTIVITY --------------------

    private async Task<List<WeeklyActivityDto>> CalculateWeeklyActivityAsync(Guid userId)
    {
        var today = DateTime.UtcNow.Date;
        var mondayOffset = (int)today.DayOfWeek == 0 ? 6 : (int)today.DayOfWeek - 1;
        var startOfWeek = today.AddDays(-mondayOffset);

        var result = new List<WeeklyActivityDto>();

        for (int i = 0; i < 7; i++)
        {
            var date = startOfWeek.AddDays(i);
            var nextDate = date.AddDays(1);
            var minutes = await CalculateDayMinutesAsync(userId, date, nextDate);

            result.Add(new WeeklyActivityDto
            {
                DayOfWeek = date.ToString("ddd"),
                Date = date,
                WorkoutMinutes = minutes,
                WorkoutHours = Math.Round(minutes / 60.0, 1),
                HasActivity = minutes > 0
            });
        }

        return result;
    }

    private async Task<int> CalculateDayMinutesAsync(Guid userId, DateTime date, DateTime nextDate)
    {
        var checkIns = await checkInRepository.Table
            .Where(c => c.UserId == userId
                && c.CheckInTime >= date && c.CheckInTime < nextDate
                && c.CheckOutTime != null)
            .ToListAsync();

        var minutes = (int)checkIns.Sum(c => (c.CheckOutTime!.Value - c.CheckInTime).TotalMinutes);

        var withoutCheckout = await checkInRepository.Table
            .CountAsync(c => c.UserId == userId
                && c.CheckInTime >= date && c.CheckInTime < nextDate
                && c.CheckOutTime == null && c.Status == CheckInStatus.Success);

        return minutes + (withoutCheckout * 60);
    }

    #endregion

    #region -------------------- TIMELINE --------------------

    private async Task<List<ActivityTimelineDto>> BuildTimelineAsync(Guid userId, DateTime startDate, DateTime endDate)
    {
        var checkIns = await GetCheckInsForTimelineAsync(userId, startDate, endDate);
        var sessions = await GetSessionsForTimelineAsync(userId, startDate, endDate);

        var allActivities = MapCheckInsToTimeline(checkIns)
            .Concat(MapSessionsToTimeline(sessions))
            .ToList();

        return allActivities
            .GroupBy(a => a.Date)
            .OrderByDescending(g => g.Key)
            .Select(g => new ActivityTimelineDto
            {
                Date = g.Key,
                DateLabel = g.Key.ToString("dddd, MMM d"),
                ActivitiesCount = g.Count(),
                ActivitiesCountLabel = g.Count() == 1 ? "1 activity" : $"{g.Count()} activities",
                ActivityTypes = g.Select(a => a.Activity.Icon).Distinct().ToList(),
                Activities = g.Select(a => a.Activity).OrderByDescending(a => a.Time).ToList()
            })
            .ToList();
    }

    private async Task<List<CheckInLog>> GetCheckInsForTimelineAsync(Guid userId, DateTime startDate, DateTime endDate)
    {
        return await checkInRepository.Table.AsNoTracking()
            .Where(c => c.UserId == userId
                && c.CheckInTime >= startDate && c.CheckInTime <= endDate
                && c.Status == CheckInStatus.Success)
            .Include(c => c.Gym)
            .OrderByDescending(c => c.CheckInTime)
            .ToListAsync();
    }

    private async Task<List<TrainingSession>> GetSessionsForTimelineAsync(Guid userId, DateTime startDate, DateTime endDate)
    {
        return await sessionRepository.Table.AsNoTracking()
            .Where(s => s.UserId == userId
                && s.UsedAt >= startDate && s.UsedAt <= endDate
                && s.Status == SessionStatus.Used)
            .Include(s => s.Trainer).ThenInclude(t => t.User)
            .Include(s => s.Gym)
            .OrderByDescending(s => s.UsedAt)
            .ToListAsync();
    }

    private static IEnumerable<(DateTime Date, TimelineActivityDto Activity)> MapCheckInsToTimeline(List<CheckInLog> checkIns)
    {
        return checkIns.Select(c =>
        {
            var duration = c.CheckOutTime.HasValue
                ? (int)(c.CheckOutTime.Value - c.CheckInTime).TotalMinutes
                : 60;

            return (c.CheckInTime.Date, new TimelineActivityDto
            {
                Id = c.Id,
                Type = "gym-checkin",
                Title = "Gym Check-in",
                Subtitle = $"{c.Gym?.Name ?? "Gym"} • {c.CheckInTime:HH:mm} • {duration} min",
                GymName = c.Gym?.Name,
                TrainerName = null,
                Time = c.CheckInTime.ToString("HH:mm"),
                Duration = duration,
                DurationLabel = $"{duration} min",
                Icon = "gym"
            });
        });
    }

    private static IEnumerable<(DateTime Date, TimelineActivityDto Activity)> MapSessionsToTimeline(List<TrainingSession> sessions)
    {
        return sessions.Select(s => (s.UsedAt!.Value.Date, new TimelineActivityDto
        {
            Id = s.Id,
            Type = "training-session",
            Title = $"{s.Trainer?.Specialization ?? "Training"} Session",
            Subtitle = $"with {s.Trainer?.User?.FullName ?? "Trainer"} • {s.UsedAt:HH:mm} • 60 min",
            GymName = s.Gym?.Name,
            TrainerName = s.Trainer?.User?.FullName,
            Time = s.UsedAt!.Value.ToString("HH:mm"),
            Duration = 60,
            DurationLabel = "60 min",
            Icon = "trainer"
        }));
    }

    #endregion

    #region -------------------- ACHIEVEMENTS --------------------

    private static List<MonthlyAchievementDto> BuildAchievements(ActivityStatsDto stats)
    {
        return
        [
            CreateAchievement("consistency", "Consistency Goal", stats.GymVisits, 20, "target"),
            CreateAchievement("hours", "Training Hours", (int)stats.HoursTrained, 30, "clock"),
            CreateAchievement("sessions", "PT Sessions", stats.PtSessions, 8, "user")
        ];
    }

    private static MonthlyAchievementDto CreateAchievement(string type, string title, int current, int target, string icon)
    {
        return new MonthlyAchievementDto
        {
            Type = type,
            Title = title,
            Current = current,
            Target = target,
            ProgressLabel = $"{current}/{target}",
            ProgressPercent = Math.Min(100, Math.Round(current / (double)target * 100, 1)),
            Icon = icon
        };
    }

    #endregion

    #region -------------------- LEGACY SUPPORT --------------------

    private async Task<List<UserActivityDto>> GetUnifiedActivitiesAsync(Guid userId, HistoryFilterDto filter)
    {
        var activities = new List<UserActivityDto>();

        if (!filter.ActivityType.HasValue || filter.ActivityType == ActivityType.Payment)
            activities.AddRange(await GetPaymentActivitiesAsync(userId, filter));

        if (!filter.ActivityType.HasValue || filter.ActivityType == ActivityType.CheckIn)
            activities.AddRange(await GetCheckInActivitiesAsync(userId, filter));

        if (!filter.ActivityType.HasValue || filter.ActivityType == ActivityType.Session)
            activities.AddRange(await GetSessionActivitiesAsync(userId, filter));

        activities = activities.OrderByDescending(a => a.Date).ToList();

        if (filter.Status.HasValue)
            activities = activities.Where(a => a.Status == filter.Status.Value).ToList();

        return activities;
    }

    private async Task<List<UserActivityDto>> GetPaymentActivitiesAsync(Guid userId, HistoryFilterDto filter)
    {
        IQueryable<Payment> query = paymentRepository.Table.AsNoTracking()
            .Where(p => p.UserId == userId)
            .Include(p => p.Package).ThenInclude(pkg => pkg.Gym);

        if (filter.StartDate.HasValue) query = query.Where(p => p.CreatedAt >= filter.StartDate.Value);
        if (filter.EndDate.HasValue) query = query.Where(p => p.CreatedAt <= filter.EndDate.Value);

        return (await query.ToListAsync()).Select(MapPaymentToActivity).ToList();
    }

    private async Task<List<UserActivityDto>> GetCheckInActivitiesAsync(Guid userId, HistoryFilterDto filter)
    {
        IQueryable<CheckInLog> query = checkInRepository.Table.AsNoTracking()
            .Where(c => c.UserId == userId)
            .Include(c => c.Gym);

        if (filter.StartDate.HasValue) query = query.Where(c => c.CheckInTime >= filter.StartDate.Value);
        if (filter.EndDate.HasValue) query = query.Where(c => c.CheckInTime <= filter.EndDate.Value);

        return (await query.ToListAsync()).Select(MapCheckInToActivity).ToList();
    }

    private async Task<List<UserActivityDto>> GetSessionActivitiesAsync(Guid userId, HistoryFilterDto filter)
    {
        IQueryable<TrainingSession> query = sessionRepository.Table.AsNoTracking()
            .Where(s => s.UserId == userId && s.Status == SessionStatus.Used)
            .Include(s => s.Trainer).ThenInclude(t => t.User)
            .Include(s => s.Gym);

        if (filter.StartDate.HasValue) query = query.Where(s => s.UsedAt >= filter.StartDate.Value);
        if (filter.EndDate.HasValue) query = query.Where(s => s.UsedAt <= filter.EndDate.Value);

        return (await query.ToListAsync()).Select(MapSessionToActivity).ToList();
    }

    private async Task<HistorySummaryDto> GetSummaryForUserAsync(Guid userId)
    {
        var paymentStats = await paymentRepository.Table.AsNoTracking()
            .Where(p => p.UserId == userId)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Spent = g.Where(p => p.Status == PaymentStatus.Successful).Sum(p => p.Amount),
                Successful = g.Count(p => p.Status == PaymentStatus.Successful),
                Pending = g.Count(p => p.Status == PaymentStatus.Pending),
                Failed = g.Count(p => p.Status == PaymentStatus.Failed)
            })
            .FirstOrDefaultAsync();

        var checkInStats = await checkInRepository.Table.AsNoTracking()
            .Where(c => c.UserId == userId)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Successful = g.Count(c => c.Status == CheckInStatus.Success),
                Failed = g.Count(c => c.Status == CheckInStatus.Failed || c.Status == CheckInStatus.Expired)
            })
            .FirstOrDefaultAsync();

        return new HistorySummaryDto
        {
            TotalPayments = paymentStats?.Total ?? 0,
            TotalCheckIns = checkInStats?.Total ?? 0,
            TotalSpent = paymentStats?.Spent ?? 0,
            SuccessfulCount = (paymentStats?.Successful ?? 0) + (checkInStats?.Successful ?? 0),
            PendingCount = paymentStats?.Pending ?? 0,
            FailedCount = (paymentStats?.Failed ?? 0) + (checkInStats?.Failed ?? 0)
        };
    }

    #endregion

    #region -------------------- MAPPERS --------------------

    private static UserActivityDto MapPaymentToActivity(Payment p) => new()
    {
        Id = p.Id,
        ActivityType = ActivityType.Payment,
        Title = $"{p.Package?.Name ?? "Paket"} öd?ni?i",
        Description = $"{p.Package?.Gym?.Name ?? "Zal"} - {p.Package?.DurationInDays ?? 0} günlük",
        Amount = p.Amount,
        Status = MapPaymentStatus(p.Status),
        StatusText = GetStatusText(MapPaymentStatus(p.Status)),
        Date = p.CreatedAt,
        Icon = "payment",
        Color = GetStatusColor(MapPaymentStatus(p.Status)),
        GymName = p.Package?.Gym?.Name,
        PackageName = p.Package?.Name
    };

    private static UserActivityDto MapCheckInToActivity(CheckInLog c) => new()
    {
        Id = c.Id,
        ActivityType = ActivityType.CheckIn,
        Title = "Zala giri?",
        Description = c.Gym?.Name ?? "Zal",
        Amount = null,
        Status = MapCheckInStatus(c.Status),
        StatusText = GetStatusText(MapCheckInStatus(c.Status)),
        Date = c.CheckInTime,
        Icon = "check-in",
        Color = GetStatusColor(MapCheckInStatus(c.Status)),
        GymName = c.Gym?.Name,
        PackageName = null
    };

    private static UserActivityDto MapSessionToActivity(TrainingSession s) => new()
    {
        Id = s.Id,
        ActivityType = ActivityType.Session,
        Title = $"{s.Trainer?.Specialization ?? "Training"} Session",
        Description = $"with {s.Trainer?.User?.FullName ?? "Trainer"}",
        Amount = s.Price,
        Status = ActivityStatus.Success,
        StatusText = GetStatusText(ActivityStatus.Success),
        Date = s.UsedAt ?? s.CreatedAt,
        Icon = "session",
        Color = GetStatusColor(ActivityStatus.Success),
        GymName = s.Gym?.Name,
        PackageName = null
    };

    private static ActivityStatus MapPaymentStatus(PaymentStatus s) => s switch
    {
        PaymentStatus.Pending => ActivityStatus.Pending,
        PaymentStatus.Successful => ActivityStatus.Success,
        PaymentStatus.Failed => ActivityStatus.Failed,
        PaymentStatus.Refunded => ActivityStatus.Refunded,
        _ => ActivityStatus.Pending
    };

    private static ActivityStatus MapCheckInStatus(CheckInStatus s) => s switch
    {
        CheckInStatus.Success => ActivityStatus.Success,
        CheckInStatus.Failed or CheckInStatus.Expired => ActivityStatus.Failed,
        _ => ActivityStatus.Pending
    };

    private static string GetStatusText(ActivityStatus s) => s switch
    {
        ActivityStatus.Pending => "Gözl?nil?n",
        ActivityStatus.Success => "U?urlu",
        ActivityStatus.Failed => "U?ursuz",
        ActivityStatus.Refunded => "Qaytar?lan",
        _ => "Nam?lum"
    };

    private static string GetStatusColor(ActivityStatus s) => s switch
    {
        ActivityStatus.Pending => "#FFC107",
        ActivityStatus.Success => "#4CAF50",
        ActivityStatus.Failed => "#F44336",
        ActivityStatus.Refunded => "#9C27B0",
        _ => "#9E9E9E"
    };

    #endregion

    #region -------------------- HELPERS --------------------

    private Guid GetCurrentUserId()
    {
        var claim = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(claim) || !Guid.TryParse(claim, out var userId))
            throw BusinessException.UnauthorizedAccess();
        return userId;
    }

    private static (DateTime start, DateTime end, string label) GetPeriodDates(string period)
    {
        var now = DateTime.UtcNow.Date;
        var mondayOffset = (int)now.DayOfWeek == 0 ? 6 : (int)now.DayOfWeek - 1;

        return period.ToLower() switch
        {
            "week" => (now.AddDays(-mondayOffset), now, "This Week"),
            "month" => (new DateTime(now.Year, now.Month, 1), now, "This Month"),
            "year" => (new DateTime(now.Year, 1, 1), now, "This Year"),
            "all" => (DateTime.MinValue, now, "All Time"),
            _ => (now.AddDays(-7), now, "This Week")
        };
    }

    #endregion
}
