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
    IHttpContextAccessor httpContextAccessor) : IHistoryService
{
    private Guid GetCurrentUserId()
    {
        var userIdClaim = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            throw BusinessException.UnauthorizedAccess();
        return userId;
    }

    public async Task<ResponseModel<HistoryResponseDto>> GetMyHistoryAsync(HistoryFilterDto filter)
    {
        var userId = GetCurrentUserId();

        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);

        var activities = await GetUnifiedActivitiesAsync(userId, filter);

        var totalCount = activities.Count;

        var paginatedActivities = activities
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var summary = await GetSummaryForUserAsync(userId);

        var response = new HistoryResponseDto
        {
            Activities = paginatedActivities,
            TotalCount = totalCount,
            CurrentPage = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            Summary = summary
        };

        return ResponseModel<HistoryResponseDto>.SuccessResponse(response);
    }

    public async Task<ResponseModel<HistorySummaryDto>> GetHistorySummaryAsync()
    {
        var userId = GetCurrentUserId();
        var summary = await GetSummaryForUserAsync(userId);
        return ResponseModel<HistorySummaryDto>.SuccessResponse(summary);
    }

    public async Task<ResponseModel<List<UserActivityDto>>> GetRecentActivitiesAsync(int count = 10)
    {
        var userId = GetCurrentUserId();
        count = Math.Clamp(count, 1, 50);

        var activities = await GetUnifiedActivitiesAsync(userId, new HistoryFilterDto());
        var recentActivities = activities.Take(count).ToList();

        return ResponseModel<List<UserActivityDto>>.SuccessResponse(recentActivities);
    }

    private async Task<List<UserActivityDto>> GetUnifiedActivitiesAsync(Guid userId, HistoryFilterDto filter)
    {
        IQueryable<Payment> paymentQuery = paymentRepository.Table
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .Include(p => p.Package)
                .ThenInclude(pkg => pkg.Gym);

        IQueryable<CheckInLog> checkInQuery = checkInRepository.Table
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .Include(c => c.Gym);

        // Apply date filters at database level
        if (filter.StartDate.HasValue)
        {
            paymentQuery = paymentQuery.Where(p => p.CreatedAt >= filter.StartDate.Value);
            checkInQuery = checkInQuery.Where(c => c.CheckInTime >= filter.StartDate.Value);
        }

        if (filter.EndDate.HasValue)
        {
            paymentQuery = paymentQuery.Where(p => p.CreatedAt <= filter.EndDate.Value);
            checkInQuery = checkInQuery.Where(c => c.CheckInTime <= filter.EndDate.Value);
        }

        var activities = new List<UserActivityDto>();

        if (!filter.ActivityType.HasValue || filter.ActivityType == ActivityType.Payment)
        {
            var payments = await paymentQuery.ToListAsync();
            var paymentDtos = payments.Select(MapPaymentToActivity);
            activities.AddRange(paymentDtos);
        }

        // Fetch and map check-ins (if not filtered out)
        if (!filter.ActivityType.HasValue || filter.ActivityType == ActivityType.CheckIn)
        {
            var checkIns = await checkInQuery.ToListAsync();
            var checkInDtos = checkIns.Select(MapCheckInToActivity);
            activities.AddRange(checkInDtos);
        }

        // Sort by date descending
        activities = activities.OrderByDescending(a => a.Date).ToList();

        // Apply status filter in memory
        if (filter.Status.HasValue)
        {
            activities = activities.Where(a => a.Status == filter.Status.Value).ToList();
        }

        return activities;
    }

    private static UserActivityDto MapPaymentToActivity(Payment payment)
    {
        var status = MapPaymentStatus(payment.Status);
        return new UserActivityDto
        {
            Id = payment.Id,
            ActivityType = ActivityType.Payment,
            Title = $"{payment.Package?.Name ?? "Paket"} öd?ni?i",
            Description = $"{payment.Package?.Gym?.Name ?? "Zal"} - {payment.Package?.DurationInDays ?? 0} günlük",
            Amount = payment.Amount,
            Status = status,
            StatusText = GetStatusText(status),
            Date = payment.CreatedAt,
            Icon = "payment",
            Color = GetStatusColor(status),
            GymName = payment.Package?.Gym?.Name,
            PackageName = payment.Package?.Name
        };
    }

    private static UserActivityDto MapCheckInToActivity(CheckInLog checkIn)
    {
        var status = MapCheckInStatus(checkIn.Status);
        return new UserActivityDto
        {
            Id = checkIn.Id,
            ActivityType = ActivityType.CheckIn,
            Title = "Zala giri?",
            Description = checkIn.Gym?.Name ?? "Zal",
            Amount = null,
            Status = status,
            StatusText = GetStatusText(status),
            Date = checkIn.CheckInTime,
            Icon = "check-in",
            Color = GetStatusColor(status),
            GymName = checkIn.Gym?.Name,
            PackageName = null
        };
    }

    private async Task<HistorySummaryDto> GetSummaryForUserAsync(Guid userId)
    {
        var paymentStats = await paymentRepository.Table
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .GroupBy(p => 1)
            .Select(g => new
            {
                TotalPayments = g.Count(),
                TotalSpent = g.Where(p => p.Status == PaymentStatus.Successful).Sum(p => p.Amount),
                SuccessfulPayments = g.Count(p => p.Status == PaymentStatus.Successful),
                PendingPayments = g.Count(p => p.Status == PaymentStatus.Pending),
                FailedPayments = g.Count(p => p.Status == PaymentStatus.Failed)
            })
            .FirstOrDefaultAsync();

        var checkInStats = await checkInRepository.Table
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .GroupBy(c => 1)
            .Select(g => new
            {
                TotalCheckIns = g.Count(),
                SuccessfulCheckIns = g.Count(c => c.Status == CheckInStatus.Success),
                FailedCheckIns = g.Count(c => c.Status == CheckInStatus.Failed || c.Status == CheckInStatus.Expired)
            })
            .FirstOrDefaultAsync();

        return new HistorySummaryDto
        {
            TotalPayments = paymentStats?.TotalPayments ?? 0,
            TotalCheckIns = checkInStats?.TotalCheckIns ?? 0,
            TotalSpent = paymentStats?.TotalSpent ?? 0,
            SuccessfulCount = (paymentStats?.SuccessfulPayments ?? 0) + (checkInStats?.SuccessfulCheckIns ?? 0),
            PendingCount = paymentStats?.PendingPayments ?? 0,
            FailedCount = (paymentStats?.FailedPayments ?? 0) + (checkInStats?.FailedCheckIns ?? 0)
        };
    }

    private static ActivityStatus MapPaymentStatus(PaymentStatus status) => status switch
    {
        PaymentStatus.Pending => ActivityStatus.Pending,
        PaymentStatus.Successful => ActivityStatus.Success,
        PaymentStatus.Failed => ActivityStatus.Failed,
        PaymentStatus.Refunded => ActivityStatus.Refunded,
        _ => ActivityStatus.Pending
    };

    private static ActivityStatus MapCheckInStatus(CheckInStatus status) => status switch
    {
        CheckInStatus.Success => ActivityStatus.Success,
        CheckInStatus.Failed => ActivityStatus.Failed,
        CheckInStatus.Expired => ActivityStatus.Failed,
        _ => ActivityStatus.Pending
    };

    private static string GetStatusText(ActivityStatus status) => status switch
    {
        ActivityStatus.Pending => "Gözl?nil?n",
        ActivityStatus.Success => "U?urlu",
        ActivityStatus.Failed => "U?ursuz",
        ActivityStatus.Refunded => "Qaytar?lan",
        _ => "Nam?lum"
    };

    private static string GetStatusColor(ActivityStatus status) => status switch
    {
        ActivityStatus.Pending => "#FFC107",
        ActivityStatus.Success => "#4CAF50",
        ActivityStatus.Failed => "#F44336",
        ActivityStatus.Refunded => "#9C27B0",
        _ => "#9E9E9E"
    };
}
