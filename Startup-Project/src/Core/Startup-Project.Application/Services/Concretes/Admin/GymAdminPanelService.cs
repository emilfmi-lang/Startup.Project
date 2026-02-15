using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Startup_Project.Application.Dtos.Admin;
using Startup_Project.Application.Dtos.Common;
using Startup_Project.Application.Interfaces;
using Startup_Project.Application.Models;
using Startup_Project.Application.Services.Interfaces.Admin;
using Startup_Project.Domain.Abstractions.Repositories;
using Startup_Project.Domain.Entities;
using Startup_Project.Domain.Entities.Identity;
using Startup_Project.Domain.Exceptions;
using System.Security.Claims;

namespace Startup_Project.Application.Services.Concretes.Admin;

/// <summary>
/// Gym Admin Panel Service
/// Bir admin = bir zal prinsipi
/// JWT-den admin tapilir -> adminin zali tapilir -> data qaytarilir
/// </summary>
public class GymAdminPanelService(
    UserManager<AppUser> userManager,
    IApplicationDbContext context,
    IGenericRepository<Trainer> trainerRepository,
    IGenericRepository<CheckInLog> checkInLogRepository,
    IGenericRepository<Package> packageRepository,
    IGenericRepository<Payment> paymentRepository,
    IMapper mapper,
    IHttpContextAccessor httpContextAccessor) : IGymAdminPanelService
{
    #region ==================== PRIVATE HELPERS ====================

    private Guid GetCurrentUserId()
    {
        var userIdClaim = httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            throw BusinessException.UnauthorizedAccess();

        return userId;
    }

    /// <summary>
    /// Adminin sahib oldugu zali tapir + OwnerId qaytarir
    /// Bir admin = bir zal prinsipi
    /// </summary>
    private async Task<(Gym Gym, Guid OwnerId)> GetAdminGymWithOwnerAsync()
    {
        var ownerId = GetCurrentUserId();

        var gym = await context.Gyms
            .AsNoTracking()
            .Include(g => g.OpeningHours)
            .FirstOrDefaultAsync(g => g.OwnerId == ownerId && !g.IsDeleted);

        if (gym is null)
            throw NotFoundException.GymForOwner();

        return (gym, ownerId);
    }

    /// <summary>
    /// Bu gym-e aid paket ID-lerini qaytarir
    /// </summary>
    private async Task<List<Guid>> GetGymPackageIdsAsync(Guid gymId)
    {
        return await packageRepository.Table
            .Where(p => p.GymId == gymId && !p.IsDeleted)
            .Select(p => p.Id)
            .ToListAsync();
    }

    #endregion

    #region ==================== 1. DASHBOARD ====================

    public async Task<ResponseModel<GymAdminDashboardDto>> GetDashboardAsync()
    {
        var (gym, ownerId) = await GetAdminGymWithOwnerAsync();
        var gymId = gym.Id;

        var gymPackageIds = await GetGymPackageIdsAsync(gymId);

        var now = DateTime.UtcNow;
        var today = now.Date;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);

        // EF Core DbContext tek thread-de isleyir — sorgular ardicil icra olunmalidir

        // Trainers table-dan trainer olan user ID-leri
        var dashboardTrainerTableIds = await trainerRepository.Table
            .Where(t => t.GymId == gymId && !t.IsDeleted)
            .Select(t => t.UserId)
            .ToListAsync();

        // AspNetUserRoles-dan "Trainer" rolu olan user ID-leri
        var dashboardTrainerRoleUsers = await userManager.GetUsersInRoleAsync("Trainer");
        var dashboardAllTrainerIds = dashboardTrainerTableIds.ToHashSet();
        dashboardAllTrainerIds.UnionWith(dashboardTrainerRoleUsers.Select(u => u.Id));

        var totalMembers = await userManager.Users
            .CountAsync(u => u.GymId == gymId && u.Id != ownerId && !dashboardAllTrainerIds.Contains(u.Id));

        var activeMembers = await context.Subscriptions
            .Where(s => gymPackageIds.Contains(s.PackageId) && s.EndDate > now)
            .Select(s => s.UserId)
            .Distinct()
            .CountAsync();

        var premiumPlans = await context.Subscriptions
            .Where(s => gymPackageIds.Contains(s.PackageId) && s.EndDate > now)
            .CountAsync();

        var checkInsTotal = await checkInLogRepository.Table
            .CountAsync(c => c.GymId == gymId);

        var todayCheckIns = await checkInLogRepository.Table
            .CountAsync(c => c.GymId == gymId && c.CheckInTime.Date == today);

        var totalTrainers = await trainerRepository.Table
            .CountAsync(t => t.GymId == gymId && !t.IsDeleted);

        var verifiedTrainers = await trainerRepository.Table
            .CountAsync(t => t.GymId == gymId && !t.IsDeleted && t.IsVerified);

        var monthlyRevenue = await paymentRepository.Table
            .Where(p => gymPackageIds.Contains(p.PackageId)
                && p.Status == Domain.Enums.PaymentStatus.Successful
                && p.CreatedAt >= startOfMonth)
            .SumAsync(p => p.Amount);

        var dto = new GymAdminDashboardDto
        {
            GymId = gymId,
            GymName = gym.Name,
            GymAddress = gym.Address,
            GymImagePath = gym.ImagePath,
            IsOpen = gym.IsOpenNow(),
            AverageRating = gym.AverageRating,
            ReviewCount = gym.ReviewCount,

            TotalMembers = totalMembers,
            ActiveMembers = activeMembers,
            PremiumPlans = premiumPlans,
            CheckInsTotal = checkInsTotal,
            TotalTrainers = totalTrainers,
            VerifiedTrainers = verifiedTrainers,
            TodayCheckIns = todayCheckIns,
            MonthlyRevenue = monthlyRevenue
        };

        return ResponseModel<GymAdminDashboardDto>.SuccessResponse(dto);
    }

    #endregion

    #region ==================== 2. MEMBERS ====================

    public async Task<ResponseModel<PaginatedResult<MemberListItemDto>>> GetMembersAsync(
        string? search = null,
        string? status = null,
        string? plan = null,
        int page = 1,
        int pageSize = 10)
    {
        var (gym, ownerId) = await GetAdminGymWithOwnerAsync();
        var gymId = gym.Id;

        // Trainers table-dan trainer olan user ID-leri
        var trainerTableUserIds = await trainerRepository.Table
            .Where(t => t.GymId == gymId && !t.IsDeleted)
            .Select(t => t.UserId)
            .ToListAsync();

        // AspNetUserRoles-dan "Trainer" rolu olan user ID-leri
        var trainerRoleUserIds = await userManager.GetUsersInRoleAsync("Trainer");
        var trainerRoleUserIdSet = trainerRoleUserIds.Select(u => u.Id).ToHashSet();

        // Her iki menbeden trainer olanlari birlesdir
        var allTrainerUserIds = trainerTableUserIds.ToHashSet();
        allTrainerUserIds.UnionWith(trainerRoleUserIdSet);

        // Base query: bu zala aid user-ler (owner ve trainer-ler xaric)
        var query = userManager.Users
            .Where(u => u.GymId == gymId && u.Id != ownerId && !allTrainerUserIds.Contains(u.Id))
            .AsNoTracking();

        // Search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(u =>
                u.Email!.ToLower().Contains(s) ||
                u.FullName.ToLower().Contains(s));
        }

        // Status filter: SQL seviyyesinde (pre-pagination)
        if (!string.IsNullOrWhiteSpace(status) && status != "all")
        {
            var normalizedStatus = status.ToLower();
            var gymPackageIds = await GetGymPackageIdsAsync(gymId);

            if (normalizedStatus == "active")
            {
                // Aktiv subscription-u olan user-ler
                var activeUserIds = await context.Subscriptions
                    .Where(s => gymPackageIds.Contains(s.PackageId) && s.EndDate > DateTime.UtcNow)
                    .Select(s => s.UserId)
                    .Distinct()
                    .ToListAsync();

                query = query.Where(u => activeUserIds.Contains(u.Id));
            }
            else if (normalizedStatus == "expired")
            {
                // Subscription-u var amma bitib
                var expiredUserIds = await context.Subscriptions
                    .Where(s => gymPackageIds.Contains(s.PackageId))
                    .GroupBy(s => s.UserId)
                    .Where(g => g.Max(s => s.EndDate) <= DateTime.UtcNow)
                    .Select(g => g.Key)
                    .ToListAsync();

                query = query.Where(u => expiredUserIds.Contains(u.Id));
            }
            else if (normalizedStatus == "inactive")
            {
                // Hec subscription-u olmayan user-ler
                var subscribedUserIds = await context.Subscriptions
                    .Where(s => gymPackageIds.Contains(s.PackageId))
                    .Select(s => s.UserId)
                    .Distinct()
                    .ToListAsync();

                query = query.Where(u => !subscribedUserIds.Contains(u.Id));
            }
        }

        // Plan filter: SQL seviyyesinde (pre-pagination)
        if (!string.IsNullOrWhiteSpace(plan) && plan != "all")
        {
            var gymPackageIds = await GetGymPackageIdsAsync(gymId);

            var planUserIds = await context.Subscriptions
                .Include(s => s.Package)
                .Where(s => gymPackageIds.Contains(s.PackageId)
                    && s.Package.Name.ToLower() == plan.ToLower())
                .Select(s => s.UserId)
                .Distinct()
                .ToListAsync();

            query = query.Where(u => planUserIds.Contains(u.Id));
        }

        var totalCount = await query.CountAsync();

        // Pagination
        var members = await query
            .OrderBy(u => u.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        if (members.Count == 0)
        {
            var emptyResult = PaginatedResult<MemberListItemDto>.Create([], totalCount, page, pageSize);
            return ResponseModel<PaginatedResult<MemberListItemDto>>.SuccessResponse(emptyResult);
        }

        // Batch data yukle (N+1 probleminin helli)
        var memberIds = members.Select(m => m.Id).ToList();

        var checkInCounts = await checkInLogRepository.Table
            .Where(c => memberIds.Contains(c.UserId) && c.GymId == gymId)
            .GroupBy(c => c.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count(), LastCheckIn = g.Max(c => c.CheckInTime) })
            .ToDictionaryAsync(x => x.UserId);

        var latestSubscriptions = await context.Subscriptions
            .Include(s => s.Package)
            .Where(s => memberIds.Contains(s.UserId) && s.Package.GymId == gymId)
            .GroupBy(s => s.UserId)
            .Select(g => g.OrderByDescending(s => s.CreatedAt).First())
            .ToDictionaryAsync(s => s.UserId);

        // DTO-lari yarat
        var memberDtos = members.Select(member =>
        {
            var dto = mapper.Map<MemberListItemDto>(member);

            // Check-in data
            if (checkInCounts.TryGetValue(member.Id, out var checkInData))
            {
                dto.CheckIns = checkInData.Count;
                dto.LastCheckIn = checkInData.LastCheckIn;
            }

            // Trainer-ler artiq base query-de xaric edilib
            dto.IsTrainer = false;

            // Subscription
            if (latestSubscriptions.TryGetValue(member.Id, out var subscription))
            {
                dto.Plan = subscription.Package?.Name;
                dto.ExpiresDate = subscription.EndDate;
                dto.Status = subscription.EndDate > DateTime.UtcNow ? "active" : "expired";
            }
            else
            {
                dto.Status = "inactive";
            }

            return dto;
        }).ToList();

        var result = PaginatedResult<MemberListItemDto>.Create(memberDtos, totalCount, page, pageSize);
        return ResponseModel<PaginatedResult<MemberListItemDto>>.SuccessResponse(result);
    }

    #endregion

    #region ==================== 3. TRAINERS ====================

    public async Task<ResponseModel<PaginatedResult<TrainerListItemDto>>> GetTrainersAsync(
        string? search = null,
        int page = 1,
        int pageSize = 10)
    {
        var (gym, _) = await GetAdminGymWithOwnerAsync();
        var gymId = gym.Id;

        // 1) Trainers table-dan olan trainer-ler (tam data: specialization, bio, rating...)
        var trainersFromTable = await trainerRepository.Table
            .Include(t => t.User)
            .Where(t => t.GymId == gymId && !t.IsDeleted)
            .AsNoTracking()
            .ToListAsync();

        var trainerTableUserIds = trainersFromTable.Select(t => t.UserId).ToHashSet();

        // 2) AspNetUserRoles-dan "Trainer" rolu olan, amma Trainers table-da OLMAYAN user-ler
        //    (database-den manual rol verilmis user-ler)
        var trainerRoleUsers = await userManager.GetUsersInRoleAsync("Trainer");
        var roleOnlyUsers = trainerRoleUsers
            .Where(u => u.GymId == gymId && !trainerTableUserIds.Contains(u.Id))
            .ToList();

        // 3) Her iki menbeni birlesdirb DTO-lari yarat
        var allTrainerDtos = new List<TrainerListItemDto>();

        // Trainers table-dan gelenler (tam data)
        allTrainerDtos.AddRange(trainersFromTable.Select(t => new TrainerListItemDto
        {
            Id = t.Id,
            UserId = t.UserId,
            FullName = t.User.FullName,
            Email = t.User.Email!,
            ProfileImageUrl = t.User.ProfileImageUrl ?? t.ProfileImageUrl,
            Specialization = t.Specialization,
            Bio = t.Bio,
            IsVerified = t.IsVerified,
            SessionCount = t.TotalSessions,
            Rating = t.AverageRating,
            SessionPrice = t.SessionPrice,
            CreatedAt = t.CreatedAt
        }));

        // Yalniz rol ile trainer edilmis user-ler (Trainers table-da yoxdur)
        allTrainerDtos.AddRange(roleOnlyUsers.Select(u => new TrainerListItemDto
        {
            Id = Guid.Empty,
            UserId = u.Id,
            FullName = u.FullName,
            Email = u.Email!,
            ProfileImageUrl = u.ProfileImageUrl,
            Specialization = "T?yin edilm?yib",
            Bio = u.Bio,
            IsVerified = false,
            SessionCount = 0,
            Rating = 0,
            SessionPrice = 0,
            CreatedAt = u.CreatedAt
        }));

        // Search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            allTrainerDtos = allTrainerDtos
                .Where(t => t.FullName.ToLower().Contains(s)
                    || t.Email.ToLower().Contains(s)
                    || t.Specialization.ToLower().Contains(s))
                .ToList();
        }

        // Sort + Pagination
        var totalCount = allTrainerDtos.Count;
        var paginatedDtos = allTrainerDtos
            .OrderBy(t => t.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var result = PaginatedResult<TrainerListItemDto>.Create(paginatedDtos, totalCount, page, pageSize);
        return ResponseModel<PaginatedResult<TrainerListItemDto>>.SuccessResponse(result);
    }

    #endregion
}
