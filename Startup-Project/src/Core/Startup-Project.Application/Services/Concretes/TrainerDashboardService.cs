using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Startup_Project.Application.Dtos.Trainer;
using Startup_Project.Application.Interfaces;
using Startup_Project.Application.Models;
using Startup_Project.Application.Services.Interfaces;
using Startup_Project.Domain.Enums;
using Startup_Project.Domain.Exceptions;
using System.Security.Claims;

namespace Startup_Project.Application.Services.Concretes;

public class TrainerDashboardService(
    IApplicationDbContext context,
    IHttpContextAccessor httpContextAccessor) : ITrainerDashboardService
{
    public async Task<ResponseModel<TrainerDashboardDto>> GetDashboardAsync()
    {
        var trainerId = await GetCurrentTrainerIdAsync();
        var now = DateTime.UtcNow;

        var stats = await BuildStatsAsync(trainerId, now);
        var upcomingSessions = await BuildUpcomingSessionsAsync(trainerId, now);
        var topStudents = await BuildTopStudentsAsync(trainerId);

        return ResponseModel<TrainerDashboardDto>.SuccessResponse(new TrainerDashboardDto
        {
            Stats = stats,
            UpcomingSessions = upcomingSessions,
            TopStudents = topStudents
        });
    }

    #region ==================== STATS ====================

    private async Task<TrainerDashboardStatsDto> BuildStatsAsync(Guid trainerId, DateTime now)
    {
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var startOfLastMonth = startOfMonth.AddMonths(-1);
        var endOfLastMonth = startOfMonth;

        // Active Students
        var activeStudents = await context.TrainerStudents
            .CountAsync(ts => ts.TrainerId == trainerId && !ts.IsDeleted);

        // New students this month
        var newStudentsThisMonth = await context.TrainerStudents
            .CountAsync(ts => ts.TrainerId == trainerId
                && !ts.IsDeleted
                && ts.CreatedAt >= startOfMonth);

        // Sessions this week
        var startOfWeek = now.Date.AddDays(-(int)now.DayOfWeek + (int)DayOfWeek.Monday);
        if (now.DayOfWeek == DayOfWeek.Sunday) startOfWeek = startOfWeek.AddDays(-7);
        var endOfWeek = startOfWeek.AddDays(7);

        var sessionsThisWeek = await context.ScheduledSessions
            .CountAsync(s => s.TrainerId == trainerId
                && !s.IsDeleted
                && s.ScheduledTime >= startOfWeek
                && s.ScheduledTime < endOfWeek
                && s.Status != ScheduledSessionStatus.Cancelled);

        var remainingSessionsThisWeek = await context.ScheduledSessions
            .CountAsync(s => s.TrainerId == trainerId
                && !s.IsDeleted
                && s.ScheduledTime >= now
                && s.ScheduledTime < endOfWeek
                && s.Status != ScheduledSessionStatus.Cancelled
                && s.Status != ScheduledSessionStatus.Completed);

        // Monthly Earnings — successful session payments
        var monthlyEarnings = await context.SessionPayments
            .Where(sp => sp.TrainerId == trainerId
                && sp.Status == PaymentStatus.Successful
                && sp.CreatedAt >= startOfMonth)
            .SumAsync(sp => sp.Amount);

        var lastMonthEarnings = await context.SessionPayments
            .Where(sp => sp.TrainerId == trainerId
                && sp.Status == PaymentStatus.Successful
                && sp.CreatedAt >= startOfLastMonth
                && sp.CreatedAt < endOfLastMonth)
            .SumAsync(sp => sp.Amount);

        var earningsChangePercent = lastMonthEarnings > 0
            ? Math.Round((double)((monthlyEarnings - lastMonthEarnings) / lastMonthEarnings * 100), 1)
            : 0;

        // Average Rating & Review Count
        var trainer = await context.Trainers
            .AsNoTracking()
            .Where(t => t.Id == trainerId)
            .Select(t => new { t.AverageRating, t.ReviewCount })
            .FirstAsync();

        return new TrainerDashboardStatsDto
        {
            ActiveStudents = activeStudents,
            NewStudentsThisMonth = newStudentsThisMonth,
            SessionsThisWeek = sessionsThisWeek,
            RemainingSessionsThisWeek = remainingSessionsThisWeek,
            MonthlyEarnings = monthlyEarnings,
            EarningsChangePercent = (decimal)earningsChangePercent,
            AverageRating = Math.Round(trainer.AverageRating, 1),
            TotalReviews = trainer.ReviewCount
        };
    }

    #endregion

    #region ==================== UPCOMING SESSIONS ====================

    private async Task<List<UpcomingSessionDto>> BuildUpcomingSessionsAsync(Guid trainerId, DateTime now)
    {
        var sessions = await context.ScheduledSessions
            .AsNoTracking()
            .Include(s => s.Member)
            .Where(s => s.TrainerId == trainerId
                && !s.IsDeleted
                && s.ScheduledTime >= now.Date
                && s.Status != ScheduledSessionStatus.Cancelled
                && s.Status != ScheduledSessionStatus.Completed)
            .OrderBy(s => s.ScheduledTime)
            .Take(10)
            .ToListAsync();

        return sessions.Select(s =>
        {
            var studentName = s.Member?.FullName ?? "Aç?q sessiya";
            return new UpcomingSessionDto
            {
                Id = s.Id,
                StudentName = studentName,
                StudentInitial = studentName.Length > 0 ? studentName[..1].ToUpperInvariant() : null,
                ExerciseType = s.ExerciseType,
                Title = s.Title,
                StartTime = s.ScheduledTime.ToString("HH:mm"),
                EndTime = s.EndTime.ToString("HH:mm"),
                DateLabel = GetDateLabel(s.ScheduledTime, now),
                Status = s.Status.ToString()
            };
        }).ToList();
    }

    private static string GetDateLabel(DateTime sessionDate, DateTime now)
    {
        var diff = (sessionDate.Date - now.Date).Days;
        return diff switch
        {
            0 => "Today",
            1 => "Tomorrow",
            _ => sessionDate.ToString("dd MMM")
        };
    }

    #endregion

    #region ==================== TOP STUDENTS ====================

    private async Task<List<TopStudentDto>> BuildTopStudentsAsync(Guid trainerId)
    {
        var students = await context.TrainerStudents
            .AsNoTracking()
            .Include(ts => ts.Student)
            .Where(ts => ts.TrainerId == trainerId && !ts.IsDeleted)
            .ToListAsync();

        if (students.Count == 0)
            return [];

        var studentIds = students.Select(ts => ts.StudentId).ToList();

        var sessionStats = await context.ScheduledSessions
            .AsNoTracking()
            .Where(s => s.TrainerId == trainerId
                && s.MemberId != null
                && studentIds.Contains(s.MemberId!.Value)
                && !s.IsDeleted
                && s.Status != ScheduledSessionStatus.Cancelled)
            .GroupBy(s => s.MemberId!.Value)
            .Select(g => new
            {
                StudentId = g.Key,
                Total = g.Count(),
                Completed = g.Count(s => s.Status == ScheduledSessionStatus.Completed)
            })
            .ToListAsync();

        return students
            .Select(ts =>
            {
                var stat = sessionStats.FirstOrDefault(s => s.StudentId == ts.StudentId);
                var total = stat?.Total ?? 0;
                var completed = stat?.Completed ?? 0;
                var attendance = total > 0 ? Math.Round((double)completed / total * 100, 0) : 0;

                return new TopStudentDto
                {
                    StudentId = ts.StudentId,
                    FullName = ts.Student.FullName,
                    Initial = ts.Student.FullName.Length > 0 ? ts.Student.FullName[..1].ToUpperInvariant() : null,
                    TotalSessions = total,
                    CompletedSessions = completed,
                    AttendancePercent = attendance
                };
            })
            .OrderByDescending(s => s.TotalSessions)
            .Take(5)
            .ToList();
    }

    #endregion

    #region ==================== HELPERS ====================

    private Guid GetCurrentUserId()
    {
        var claim = httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(claim) || !Guid.TryParse(claim, out var userId))
            throw BusinessException.UnauthorizedAccess();

        return userId;
    }

    private async Task<Guid> GetCurrentTrainerIdAsync()
    {
        var userId = GetCurrentUserId();

        var trainerId = await context.Trainers
            .Where(t => t.UserId == userId && !t.IsDeleted)
            .Select(t => t.Id)
            .FirstOrDefaultAsync();

        if (trainerId == Guid.Empty)
            throw BusinessException.ValidationFailed("Trainer profili tap?lmad?.");

        return trainerId;
    }

    #endregion
}
