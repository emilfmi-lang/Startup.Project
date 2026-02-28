using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Startup_Project.Application.Dtos.Dashboard;
using Startup_Project.Application.Interfaces;
using Startup_Project.Application.Models;
using Startup_Project.Application.Services.Interfaces;
using Startup_Project.Domain.Enums;
using Startup_Project.Domain.Exceptions;
using System.Security.Claims;

namespace Startup_Project.Application.Services.Concretes;

public class UserDashboardService(
    IApplicationDbContext context,
    IHttpContextAccessor httpContextAccessor) : IUserDashboardService
{
    private Guid GetCurrentUserId()
    {
        var userIdClaim = httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            throw BusinessException.UnauthorizedAccess();

        return userId;
    }

    public async Task<ResponseModel<UserDashboardDto>> GetDashboardAsync()
    {
        var userId = GetCurrentUserId();
        var now = DateTime.UtcNow;

        var activeSubscription = await BuildActiveSubscriptionAsync(userId, now);
        var thisMonth = await BuildThisMonthStatsAsync(userId, now);
        var recentActivities = await BuildRecentActivitiesAsync(userId);

        var dto = new UserDashboardDto
        {
            ActiveSubscription = activeSubscription,
            ThisMonth = thisMonth,
            RecentActivities = recentActivities
        };

        return ResponseModel<UserDashboardDto>.SuccessResponse(dto);
    }

    private async Task<ActiveSubscriptionDto?> BuildActiveSubscriptionAsync(Guid userId, DateTime now)
    {
        var subscription = await context.Subscriptions
            .AsNoTracking()
            .Where(s => s.UserId == userId
                && !s.IsDeleted
                && s.Status == SubscriptionStatus.Active
                && s.EndDate >= now)
            .Include(s => s.Package)
                .ThenInclude(p => p.Gym)
            .OrderByDescending(s => s.EndDate)
            .FirstOrDefaultAsync();

        if (subscription is null)
            return null;

        var package = subscription.Package;
        var gym = package.Gym;

        var totalDays = Math.Max(1, (int)(subscription.EndDate - subscription.StartDate).TotalDays);
        var elapsedDays = Math.Max(0, (int)(now - subscription.StartDate).TotalDays);
        var daysRemaining = Math.Max(0, (int)(subscription.EndDate - now).TotalDays);
        var progressPercent = Math.Clamp(Math.Round((double)elapsedDays / totalDays * 100, 1), 0, 100);

        return new ActiveSubscriptionDto
        {
            SubscriptionId = subscription.Id,
            PackageName = package.Name,
            GymName = gym.Name,
            GymImage = gym.ImagePath,
            DaysRemaining = daysRemaining,
            ElapsedDays = elapsedDays,
            TotalDays = totalDays,
            ProgressPercent = progressPercent,
            ProgressLabel = $"{elapsedDays}/{totalDays} days",
            DaysRemainingLabel = daysRemaining == 0 ? "Expires today" : $"{daysRemaining} days left",
            Status = daysRemaining <= 7 ? "Expiring Soon" : "Active",
            StartDate = subscription.StartDate,
            EndDate = subscription.EndDate
        };
    }

    private async Task<ThisMonthStatsDto> BuildThisMonthStatsAsync(Guid userId, DateTime now)
    {
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        // Gym Visits — u?urlu check-in say?
        var gymVisits = await context.CheckInLogs
            .CountAsync(c => c.UserId == userId
                && c.CheckInTime >= startOfMonth
                && c.Status == CheckInStatus.Success);

        // Total Time — check-in/check-out f?rqind?n hesablan?r
        var checkInsWithCheckout = await context.CheckInLogs
            .Where(c => c.UserId == userId
                && c.CheckInTime >= startOfMonth
                && c.CheckOutTime != null
                && c.Status == CheckInStatus.Success)
            .Select(c => new { c.CheckInTime, c.CheckOutTime })
            .ToListAsync();

        var totalMinutes = checkInsWithCheckout
            .Sum(c => (c.CheckOutTime!.Value - c.CheckInTime).TotalMinutes);

        // Checkout olmayan check-in-l?r üçün default 60 d?qiq?
        var checkInsWithoutCheckout = await context.CheckInLogs
            .CountAsync(c => c.UserId == userId
                && c.CheckInTime >= startOfMonth
                && c.CheckOutTime == null
                && c.Status == CheckInStatus.Success);

        totalMinutes += checkInsWithoutCheckout * 60;

        var totalHours = Math.Round(totalMinutes / 60, 0);

        // Sessions — öd?ni?li sessiyalar (scheduled sessions)
        var bookedSessionIds = await context.SessionPayments
            .Where(p => p.StudentId == userId
                && p.Status == PaymentStatus.Successful
                && p.CreatedAt >= startOfMonth)
            .Select(p => p.ScheduledSessionId)
            .ToListAsync();

        var sessionsCount = bookedSessionIds.Count;

        // TrainingSession-lardan da ?lav? et (legacy)
        var legacySessions = await context.TrainingSessions
            .CountAsync(s => s.UserId == userId
                && s.Status == SessionStatus.Used
                && s.UsedAt >= startOfMonth);

        sessionsCount += legacySessions;

        return new ThisMonthStatsDto
        {
            GymVisits = gymVisits,
            TotalTime = $"{(int)totalHours}h",
            TotalTimeHours = totalHours,
            Sessions = sessionsCount
        };
    }

    private async Task<List<DashboardActivityDto>> BuildRecentActivitiesAsync(Guid userId)
    {
        var activities = new List<DashboardActivityDto>();

        // Check-in-l?r
        var recentCheckIns = await context.CheckInLogs
            .AsNoTracking()
            .Where(c => c.UserId == userId && c.Status == CheckInStatus.Success)
            .Include(c => c.Gym)
            .OrderByDescending(c => c.CheckInTime)
            .Take(10)
            .ToListAsync();

        activities.AddRange(recentCheckIns.Select(c => new DashboardActivityDto
        {
            Id = c.Id,
            Type = "checkin",
            Title = $"Checked in at {c.Gym?.Name ?? "Gym"}",
            Subtitle = null,
            Icon = "checkin",
            Date = c.CheckInTime,
            DateLabel = c.CheckInTime.ToString("yyyy-MM-dd") + " at " + c.CheckInTime.ToString("HH:mm")
        }));

        // ScheduledSession-lar (öd?ni?li sessiyalar)
        var recentSessionPayments = await context.SessionPayments
            .AsNoTracking()
            .Where(p => p.StudentId == userId && p.Status == PaymentStatus.Successful)
            .Include(p => p.ScheduledSession)
                .ThenInclude(s => s.Trainer)
                    .ThenInclude(t => t.User)
            .OrderByDescending(p => p.ScheduledSession.ScheduledTime)
            .Take(10)
            .ToListAsync();

        activities.AddRange(recentSessionPayments.Select(p => new DashboardActivityDto
        {
            Id = p.ScheduledSessionId,
            Type = "session",
            Title = $"Session with {p.ScheduledSession.Trainer?.User?.FullName ?? "Trainer"}",
            Subtitle = p.ScheduledSession.ExerciseType,
            Icon = "session",
            Date = p.ScheduledSession.ScheduledTime,
            DateLabel = p.ScheduledSession.ScheduledTime.ToString("yyyy-MM-dd") + " at "
                + p.ScheduledSession.ScheduledTime.ToString("HH:mm")
        }));

        // TrainingSession-lar (legacy)
        var legacySessions = await context.TrainingSessions
            .AsNoTracking()
            .Where(s => s.UserId == userId && s.Status == SessionStatus.Used)
            .Include(s => s.Trainer)
                .ThenInclude(t => t.User)
            .Include(s => s.Gym)
            .OrderByDescending(s => s.UsedAt)
            .Take(10)
            .ToListAsync();

        activities.AddRange(legacySessions.Select(s => new DashboardActivityDto
        {
            Id = s.Id,
            Type = "session",
            Title = $"Session with {s.Trainer?.User?.FullName ?? "Trainer"}",
            Subtitle = s.Trainer?.Specialization,
            Icon = "session",
            Date = s.UsedAt ?? s.CreatedAt,
            DateLabel = (s.UsedAt ?? s.CreatedAt).ToString("yyyy-MM-dd") + " at "
                + (s.UsedAt ?? s.CreatedAt).ToString("HH:mm")
        }));

        // Tarix? gör? s?rala, dublikatlar? sil, son 10 götür
        return activities
            .DistinctBy(a => a.Id)
            .OrderByDescending(a => a.Date)
            .Take(10)
            .ToList();
    }
}
