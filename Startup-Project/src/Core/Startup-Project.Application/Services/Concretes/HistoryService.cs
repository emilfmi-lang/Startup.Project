using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Startup_Project.Application.Dtos.History;
using Startup_Project.Application.Enums;
using Startup_Project.Application.Models;
using Startup_Project.Application.Services.Interfaces;
using Startup_Project.Domain.Abstractions.Repositories;
using Startup_Project.Domain.Entities;
using Startup_Project.Domain.Enums;
using System.Security.Claims;

namespace Startup_Project.Application.Services.Concretes;

public class HistoryService : IHistoryService
{
    private readonly IGenericRepository<Payment> _paymentRepository;
    private readonly IGenericRepository<CheckInLog> _checkInRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HistoryService(
        IGenericRepository<Payment> paymentRepository,
        IGenericRepository<CheckInLog> checkInRepository,
        IHttpContextAccessor httpContextAccessor)
    {
        _paymentRepository = paymentRepository;
        _checkInRepository = checkInRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    public async Task<ResponseModel<HistoryResponseDto>> GetMyHistoryAsync(HistoryFilterDto filter)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return ResponseModel<HistoryResponseDto>.FailureResponse("?stifad?çi tap?lmad?.");

        // Validate pagination
        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);

        // Build unified query with projection (no full entity loading)
        var activities = await GetUnifiedActivitiesAsync(userId, filter);
        
        // Get total count for pagination
        var totalCount = activities.Count;
        
        // Apply pagination
        var paginatedActivities = activities
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // Get summary (single optimized query)
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
        if (userId == Guid.Empty)
            return ResponseModel<HistorySummaryDto>.FailureResponse("?stifad?çi tap?lmad?.");

        var summary = await GetSummaryForUserAsync(userId);
        return ResponseModel<HistorySummaryDto>.SuccessResponse(summary);
    }

    public async Task<ResponseModel<List<UserActivityDto>>> GetRecentActivitiesAsync(int count = 10)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return ResponseModel<List<UserActivityDto>>.FailureResponse("?stifad?çi tap?lmad?.");

        count = Math.Clamp(count, 1, 50);

        var activities = await GetUnifiedActivitiesAsync(userId, new HistoryFilterDto());
        var recentActivities = activities.Take(count).ToList();

        return ResponseModel<List<UserActivityDto>>.SuccessResponse(recentActivities);
    }

    private async Task<List<UserActivityDto>> GetUnifiedActivitiesAsync(Guid userId, HistoryFilterDto filter)
    {
        // Payment query with projection
        var paymentQuery = _paymentRepository.Table
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .Include(p => p.Package)
                .ThenInclude(pkg => pkg.Gym)
            .Select(p => new UserActivityDto
            {
                Id = p.Id,
                ActivityType = ActivityType.Payment,
                Title = $"{p.Package.Name} öd?ni?i",
                Description = $"{p.Package.Gym.Name} - {p.Package.DurationInDays} günlük",
                Amount = p.Amount,
                Status = MapPaymentStatus(p.Status),
                StatusText = GetStatusText(MapPaymentStatus(p.Status)),
                Date = p.CreatedAt,
                Icon = "payment",
                Color = GetStatusColor(MapPaymentStatus(p.Status)),
                GymName = p.Package.Gym.Name,
                PackageName = p.Package.Name
            });

        // CheckIn query with projection
        var checkInQuery = _checkInRepository.Table
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .Include(c => c.Gym)
            .Select(c => new UserActivityDto
            {
                Id = c.Id,
                ActivityType = ActivityType.CheckIn,
                Title = "Zala giri?",
                Description = c.Gym.Name,
                Amount = null,
                Status = MapCheckInStatus(c.Status),
                StatusText = GetStatusText(MapCheckInStatus(c.Status)),
                Date = c.CheckInTime,
                Icon = "check-in",
                Color = GetStatusColor(MapCheckInStatus(c.Status)),
                GymName = c.Gym.Name,
                PackageName = null
            });

        // Apply filters
        if (filter.ActivityType.HasValue)
        {
            if (filter.ActivityType == ActivityType.Payment)
                checkInQuery = checkInQuery.Where(x => false); // Exclude check-ins
            else if (filter.ActivityType == ActivityType.CheckIn)
                paymentQuery = paymentQuery.Where(x => false); // Exclude payments
        }

        if (filter.StartDate.HasValue)
        {
            paymentQuery = paymentQuery.Where(p => p.Date >= filter.StartDate.Value);
            checkInQuery = checkInQuery.Where(c => c.Date >= filter.StartDate.Value);
        }

        if (filter.EndDate.HasValue)
        {
            paymentQuery = paymentQuery.Where(p => p.Date <= filter.EndDate.Value);
            checkInQuery = checkInQuery.Where(c => c.Date <= filter.EndDate.Value);
        }

        // Execute queries
        var payments = await paymentQuery.ToListAsync();
        var checkIns = await checkInQuery.ToListAsync();

        // Union and sort
        var allActivities = payments
            .Concat(checkIns)
            .OrderByDescending(a => a.Date)
            .ToList();

        // Apply status filter after union
        if (filter.Status.HasValue)
        {
            allActivities = allActivities
                .Where(a => a.Status == filter.Status.Value)
                .ToList();
        }

        return allActivities;
    }

    private async Task<HistorySummaryDto> GetSummaryForUserAsync(Guid userId)
    {
        // Single query for payment stats
        var paymentStats = await _paymentRepository.Table
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

        // Single query for check-in stats
        var checkInStats = await _checkInRepository.Table
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
