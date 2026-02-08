using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Startup_Project.Application.Dtos.Subscriptions;
using Startup_Project.Application.Models;
using Startup_Project.Application.Services.Interfaces;
using Startup_Project.Domain.Abstractions.Repositories;
using Startup_Project.Domain.Entities;
using Startup_Project.Domain.Enums;
using Startup_Project.Domain.Exceptions;
using System.Security.Claims;

namespace Startup_Project.Application.Services.Concretes;
public class UserSubscriptionService(
    IGenericRepository<Subscription> subscriptionRepository,
    IGenericRepository<Payment> paymentRepository,
    IHttpContextAccessor httpContextAccessor) : IUserSubscriptionService
{
    private Guid GetCurrentUserId()
    {
        var userIdClaim = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            throw BusinessException.UnauthorizedAccess();
        return userId;
    }

    public async Task<ResponseModel<List<UserSubscriptionDto>>> GetMyActiveSubscriptionsAsync()
    {
        var userId = GetCurrentUserId();
        var now = DateTime.UtcNow;

        var subscriptions = await subscriptionRepository.Table
            .AsNoTracking()
            .Where(s => s.UserId == userId 
                     && !s.IsDeleted 
                     && s.Status == SubscriptionStatus.Active 
                     && s.EndDate >= now)
            .Include(s => s.Package)
                .ThenInclude(p => p.Gym)
            .Include(s => s.Package)
                .ThenInclude(p => p.Features.Where(f => !f.IsDeleted))
            .OrderByDescending(s => s.EndDate)
            .ToListAsync();

        var dtos = subscriptions.Select(s => MapToUserSubscriptionDto(s, now)).ToList();

        return ResponseModel<List<UserSubscriptionDto>>.SuccessResponse(dtos);
    }

    public async Task<ResponseModel<List<SubscriptionHistoryDto>>> GetMyPaymentHistoryAsync(int page = 1, int pageSize = 20)
    {
        var userId = GetCurrentUserId();

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var payments = await paymentRepository.Table
            .AsNoTracking()
            .Where(p => p.UserId == userId && !p.IsDeleted)
            .Include(p => p.Package)
                .ThenInclude(pkg => pkg.Gym)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = payments.Select(p => new SubscriptionHistoryDto
        {
            Id = p.Id,
            GymName = p.Package.Gym.Name,
            GymImage = p.Package.Gym.ImagePath,
            PackageName = p.Package.Name,
            Amount = p.Amount,
            AmountLabel = $"?{p.Amount:0}",
            PaymentDate = p.CreatedAt,
            PaymentDateLabel = p.CreatedAt.ToString("yyyy-MM-dd"),
            PaymentStatus = p.Status.ToString(),
            IsPaid = p.Status == PaymentStatus.Successful
        }).ToList();

        return ResponseModel<List<SubscriptionHistoryDto>>.SuccessResponse(dtos);
    }

    public async Task<ResponseModel<UserSubscriptionDto>> GetSubscriptionDetailAsync(Guid subscriptionId)
    {
        var userId = GetCurrentUserId();
        var now = DateTime.UtcNow;

        var subscription = await subscriptionRepository.Table
            .AsNoTracking()
            .Where(s => s.Id == subscriptionId && s.UserId == userId && !s.IsDeleted)
            .Include(s => s.Package)
                .ThenInclude(p => p.Gym)
            .Include(s => s.Package)
                .ThenInclude(p => p.Features.Where(f => !f.IsDeleted))
            .FirstOrDefaultAsync();

            if (subscription == null)
                throw NotFoundException.Subscription(subscriptionId);

            var dto = MapToUserSubscriptionDto(subscription, now);

            return ResponseModel<UserSubscriptionDto>.SuccessResponse(dto);
        }

        public async Task<ResponseModel<bool>> RenewSubscriptionAsync(Guid subscriptionId)
        {
            var userId = GetCurrentUserId();

            var subscription = await subscriptionRepository.Table
                .Where(s => s.Id == subscriptionId && s.UserId == userId && !s.IsDeleted)
                .Include(s => s.Package)
                .FirstOrDefaultAsync();

            if (subscription == null)
                throw NotFoundException.Subscription(subscriptionId);

            // Extend subscription by package duration
            subscription.Extend(subscription.Package.DurationInDays);
            subscriptionRepository.Update(subscription);
            await subscriptionRepository.SaveAsync();

            return ResponseModel<bool>.SuccessResponse(true);
        }

    private static UserSubscriptionDto MapToUserSubscriptionDto(Subscription subscription, DateTime now)
    {
        var package = subscription.Package;
        var gym = package.Gym;

        var totalDays = (subscription.EndDate - subscription.StartDate).TotalDays;
        var elapsedDays = (now - subscription.StartDate).TotalDays;
        var daysRemaining = Math.Max(0, (int)(subscription.EndDate - now).TotalDays);

        var progressPercent = totalDays > 0
            ? Math.Clamp((elapsedDays / totalDays) * 100, 0, 100)
            : 100;

        var status = GetSubscriptionStatus(subscription.EndDate, now);
        var isExpiringSoon = daysRemaining <= 7 && daysRemaining > 0;

        return new UserSubscriptionDto
        {
            Id = subscription.Id,
            GymId = gym.Id,
            GymName = gym.Name,
            GymImage = gym.ImagePath,
            PackageId = package.Id,
            PackageName = package.Name,
            Price = package.Price,
            PriceLabel = $"?{package.Price:0} per month",
            StartDate = subscription.StartDate,
            EndDate = subscription.EndDate,
            DateRange = $"{subscription.StartDate:yyyy-MM-dd} - {subscription.EndDate:yyyy-MM-dd}",
            Status = status,
            IsActive = subscription.IsActive,
            IsExpiringSoon = isExpiringSoon,
            DaysRemaining = daysRemaining,
            TimeRemainingLabel = daysRemaining == 0 ? "Expired" : $"{daysRemaining} days left",
            ProgressPercent = progressPercent,
            Features = package.Features
                .Where(f => !f.IsDeleted)
                .OrderBy(f => f.DisplayOrder)
                .Select(f => f.Name)
                .ToList()
        };
    }

    private static string GetSubscriptionStatus(DateTime endDate, DateTime now)
    {
        var daysRemaining = (endDate - now).Days;

        if (daysRemaining < 0)
            return "Expired";
        if (daysRemaining <= 7)
            return "Expiring Soon";
        return "Active";
    }
}
