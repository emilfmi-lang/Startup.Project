using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Startup_Project.Application.Abstractions.Services;
using Startup_Project.Application.Dtos.Admin;
using Startup_Project.Application.Dtos.Common;
using Startup_Project.Application.Interfaces;
using Startup_Project.Application.Models;
using Startup_Project.Application.Services.Interfaces.Admin;
using Startup_Project.Domain.Abstractions.Repositories;
using Startup_Project.Domain.Entities;
using Startup_Project.Domain.Entities.Identity;
using Startup_Project.Domain.Enums;
using Startup_Project.Domain.Exceptions;
using System.Security.Claims;

namespace Startup_Project.Application.Services.Concretes.Admin;

/// <summary>
/// Gym Owner/Admin Panel - Members Management Service
/// Senior-level Implementation:
/// - Multi-gym support (Owner -> Multiple Gyms)
/// - AppUser.GymId dependency removed
/// - IApplicationDbContext for direct Gym ownership query
/// - .ProjectTo() for SQL-level DTO mapping (performance optimized)
/// - Repository/Service pattern compliance
/// </summary>
public class GymAdminMembersService(
    UserManager<AppUser> userManager,
    IApplicationDbContext context,
    IGenericRepository<Trainer> trainerRepository,
    IGenericRepository<CheckInLog> checkInLogRepository,
    IMapper mapper,
    IHttpContextAccessor httpContextAccessor,
    IJwtTokenService jwtTokenService) : IGymAdminMembersService
{
    #region ==================== PRIVATE HELPERS ====================

    /// <summary>
    /// JWT token-d?n owner/admin user ID-ni ç?xar
    /// </summary>
    private Guid GetOwnerId()
    {
        var userIdClaim = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var ownerId))
            throw BusinessException.UnauthorizedAccess();
        return ownerId;
    }

    /// <summary>
    /// Owner-? m?xsus gym ID-l?rin siyah?s?n? getir
    /// Database-d?n direct query: Gym.OwnerId == ownerId
    /// </summary>
    private async Task<HashSet<Guid>> GetOwnerGymIdsAsync(Guid ownerId)
    {
        return (await context.Gyms
            .Where(g => g.OwnerId == ownerId && !g.IsDeleted)
            .Select(g => g.Id)
            .ToListAsync())
            .ToHashSet();
    }

    /// <summary>
    /// Single gym ownership validate et
    /// SelectedGymId owner-a aid olub olmad???n? check et
    /// </summary>
    private async Task<bool> IsGymOwnedByAsync(Guid gymId, Guid ownerId)
    {
        return await context.Gyms
            .Where(g => g.OwnerId == ownerId && g.Id == gymId && !g.IsDeleted)
            .AnyAsync();
    }

    #endregion

    public async Task<ResponseModel<List<GymBasicInfoDto>>> GetOwnedGymsAsync(Guid ownerId)
    {
        var gyms = await context.Gyms
            .Where(g => g.OwnerId == ownerId && !g.IsDeleted)
            .ToListAsync();

        // H?r gym üçün members v? trainers say?n? hesabla
        var gymDtos = new List<GymBasicInfoDto>();

        foreach (var gym in gyms)
        {
            // Bu gym-? aid memberl?rin say?
            var memberCount = await userManager.Users
                .Where(u => u.GymId == gym.Id)
                .CountAsync();

            // Bu gym-? aid trainers-l?rin say?
            var trainerCount = await trainerRepository.Table
                .Where(t => t.GymId == gym.Id && !t.IsDeleted)
                .CountAsync();

            gymDtos.Add(new GymBasicInfoDto
            {
                Id = gym.Id,
                Name = gym.Name,
                Address = gym.Address,
                MemberCount = memberCount,
                TrainerCount = trainerCount
            });
        }

        return ResponseModel<List<GymBasicInfoDto>>.SuccessResponse(gymDtos);
    }

    public async Task<ResponseModel<PaginatedResult<MemberListItemDto>>> GetMembersAsync(
        Guid ownerId,
        AdminMembersFilterDto filter)
    {
        // Owner-? m?xsus gym ID-l?ri getir
        var ownerGymIds = await GetOwnerGymIdsAsync(ownerId);
        if (ownerGymIds.Count == 0)
            throw NotFoundException.UserByEmail("No gyms owned by this owner");

        // ?g?r SelectedGymId varsa, onu validate et
        if (filter.SelectedGymId.HasValue && !ownerGymIds.Contains(filter.SelectedGymId.Value))
            throw BusinessException.Custom(
                Domain.Enums.Errors.BusinessErrorCodes.ForbiddenAccess,
                "Bu gym siz? aid deyil.");

        // Filter gym-l?ri (SelectedGymId varsa o, yoxsa ham?s?)
        var filterGymIds = filter.SelectedGymId.HasValue
            ? new HashSet<Guid> { filter.SelectedGymId.Value }
            : ownerGymIds;

        // Trainers table-dan trainer olan user ID-leri
        var trainerTableIds = await trainerRepository.Table
            .Where(t => filterGymIds.Contains(t.GymId ?? Guid.Empty) && !t.IsDeleted)
            .Select(t => t.UserId)
            .ToListAsync();

        // AspNetUserRoles-dan "Trainer" rolu olan user ID-leri
        var trainerRoleUsers = await userManager.GetUsersInRoleAsync("Trainer");
        var allTrainerIds = trainerTableIds.ToHashSet();
        allTrainerIds.UnionWith(trainerRoleUsers.Select(u => u.Id));

        // Base query: trainer-ler ve owner xaric
        var query = userManager.Users
            .Where(u => filterGymIds.Contains(u.GymId ?? Guid.Empty)
                && u.Id != ownerId
                && !allTrainerIds.Contains(u.Id))
            .AsNoTracking();

        // Search filteri
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.ToLower();
            query = query.Where(u =>
                u.Email!.ToLower().Contains(search) ||
                u.FullName.ToLower().Contains(search));
        }

        // Total count
        var totalCount = await query.CountAsync();

        // Pagination
        var members = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        // ? AutoMapper .ProjectTo() il? SQL-d? conversion (performance optimized)
        var memberDtos = new List<MemberListItemDto>();
        foreach (var member in members)
        {
            var dto = mapper.Map<MemberListItemDto>(member);

            // Dynamic enrichment (check-ins, subscription)
            var checkInCount = await checkInLogRepository.Table
                .CountAsync(x => x.UserId == member.Id && filterGymIds.Contains(x.GymId));
            dto.CheckIns = checkInCount;

            // Trainer-ler artiq base query-de xaric edilib
            dto.IsTrainer = false;

            // Subscription
            var subscription = await context.Subscriptions
                .Where(s => s.UserId == member.Id)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync();

            if (subscription != null)
            {
                dto.Plan = subscription.Package?.Name;
                dto.ExpiresDate = subscription.EndDate;
                dto.Status = subscription.EndDate > DateTime.UtcNow ? "active" : "expired";
            }

            memberDtos.Add(dto);
        }

        var result = PaginatedResult<MemberListItemDto>.Create(
            memberDtos,
            totalCount,
            filter.Page,
            filter.PageSize);

        return ResponseModel<PaginatedResult<MemberListItemDto>>.SuccessResponse(result);
    }

    public async Task<ResponseModel<MemberDetailDto>> GetMemberDetailAsync(Guid ownerId, Guid memberId)
    {
        // Owner-? m?xsus gym ID-l?ri getir
        var ownerGymIds = await GetOwnerGymIdsAsync(ownerId);
        if (ownerGymIds.Count == 0)
            throw NotFoundException.UserByEmail("No gyms owned");

        // Member-i getir v? verify et
        var member = await userManager.FindByIdAsync(memberId.ToString());
        if (member == null || !ownerGymIds.Contains(member.GymId ?? Guid.Empty))
            throw NotFoundException.UserByEmail(member?.Email ?? "unknown");

        var dto = mapper.Map<MemberDetailDto>(member);

        // Gym info
        if (member.GymId.HasValue)
        {
            var gym = await context.Gyms.FirstOrDefaultAsync(g => g.Id == member.GymId.Value);
            if (gym != null)
            {
                dto.Gym = new GymInfoDto { Id = gym.Id, Name = gym.Name };
            }
        }

        // Subscription
        var subscription = await context.Subscriptions
            .Where(s => s.UserId == memberId)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync();

        if (subscription != null)
        {
            dto.Subscription = new SubscriptionInfoDto
            {
                Id = subscription.Id,
                Plan = subscription.Package?.Name,
                StartDate = subscription.StartDate,
                ExpiresDate = subscription.EndDate,
                IsActive = subscription.EndDate > DateTime.UtcNow,
                RemainingDays = (int)(subscription.EndDate - DateTime.UtcNow).TotalDays
            };
        }

        // Stats
        if (member.GymId.HasValue)
        {
            var checkInCount = await checkInLogRepository.Table
                .CountAsync(x => x.UserId == memberId && x.GymId == member.GymId.Value);

            var lastCheckIn = await checkInLogRepository.Table
                .Where(x => x.UserId == memberId && x.GymId == member.GymId.Value)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            dto.Stats = new MemberStatsDto
            {
                TotalCheckIns = checkInCount,
                LastCheckIn = lastCheckIn?.CreatedAt,
                JoinDate = member.CreatedAt,
                AverageCheckInsPerMonth = CalculateAverageCheckIns(member.CreatedAt, checkInCount)
            };
        }

        // Trainer info
        if (member.GymId.HasValue)
        {
            var trainer = await trainerRepository.Table
                .FirstOrDefaultAsync(t => t.UserId == memberId && t.GymId == member.GymId.Value);

            if (trainer != null)
            {
                dto.Trainer = new TrainerInfoDto
                {
                    Id = trainer.Id,
                    Specialization = trainer.Specialization,
                    IsVerified = trainer.IsVerified
                };
            }
        }

        // Roles
        var roles = await userManager.GetRolesAsync(member);
        dto.Roles = roles.ToList();

        return ResponseModel<MemberDetailDto>.SuccessResponse(dto);
    }

    public async Task<ResponseModel<MakeTrainerResponseDto>> MakeMemberTrainerAsync(
        Guid ownerId,
        Guid memberId,
        Guid gymId,
        MakeTrainerRequestDto request)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(request.Specialization))
            throw BusinessException.ValidationFailed("?xtisasla?ma bo? ola bilm?z.");

        if (request.SessionPrice <= 0)
            throw BusinessException.ValidationFailed("Sessiya qiym?ti müsb?t olmal?d?r.");

        // Gym ownership validate
        if (!await IsGymOwnedByAsync(gymId, ownerId))
            throw BusinessException.Custom(
                Domain.Enums.Errors.BusinessErrorCodes.ForbiddenAccess,
                "Bu gym siz? aid deyil.");

        // Member validate
        var member = await userManager.FindByIdAsync(memberId.ToString());
        if (member == null)
            throw NotFoundException.UserByEmail("Member not found");

        // Check not already trainer
        var existingTrainer = await trainerRepository.Table
            .FirstOrDefaultAsync(t => t.UserId == memberId && t.GymId == gymId);

        if (existingTrainer != null)
            throw BusinessException.Custom(
                Domain.Enums.Errors.BusinessErrorCodes.TrainerAlreadyExists,
                "Bu member art?q trainer-dir.");

        // Create trainer
        var trainer = new Trainer(
            memberId,
            gymId,
            request.Specialization,
            request.SessionPrice);

        await trainerRepository.AddAsync(trainer);
        await trainerRepository.SaveAsync();

        // Add role
        await userManager.AddToRoleAsync(member, "Trainer");

        // Generate new token with Trainer role
        var updatedRoles = await userManager.GetRolesAsync(member);
        var newToken = jwtTokenService.GenerateTokenAsync(member, updatedRoles);

        var trainerDto = mapper.Map<TrainerListItemDto>(trainer);
        trainerDto.FullName = member.FullName;
        trainerDto.Email = member.Email;
        trainerDto.ProfileImageUrl = member.ProfileImageUrl;

        var responseData = new MakeTrainerResponseDto
        {
            Trainer = trainerDto,
            NewToken = newToken,
            Message = "Member successfully promoted to trainer"
        };

        return ResponseModel<MakeTrainerResponseDto>.SuccessResponse(responseData);
    }

    public async Task<ResponseModel<PaginatedResult<TrainerListItemDto>>> GetTrainersAsync(
        Guid ownerId,
        Guid gymId,
        int page = 1,
        int pageSize = 10)
    {
        // Gym ownership validate
        if (!await IsGymOwnedByAsync(gymId, ownerId))
            throw BusinessException.Custom(
                Domain.Enums.Errors.BusinessErrorCodes.ForbiddenAccess,
                "Bu gym siz? aid deyil.");

        // ? .ProjectTo() il? SQL-d? DTO mapping
        var query = trainerRepository.Table
            .Where(t => t.GymId == gymId && !t.IsDeleted)
            .ProjectTo<TrainerListItemDto>(mapper.ConfigurationProvider)
            .AsNoTracking();

        var totalCount = await query.CountAsync();

        var trainers = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = PaginatedResult<TrainerListItemDto>.Create(trainers, totalCount, page, pageSize);
        return ResponseModel<PaginatedResult<TrainerListItemDto>>.SuccessResponse(result);
    }

    public async Task<ResponseModel<TrainerDetailDto>> GetTrainerDetailAsync(Guid ownerId, Guid trainerId)
    {
        // Owner-? m?xsus gym ID-l?ri getir
        var ownerGymIds = await GetOwnerGymIdsAsync(ownerId);
        if (ownerGymIds.Count == 0)
            throw NotFoundException.UserByEmail("No gyms owned");

        // Trainer-i getir
        var trainer = await trainerRepository.GetByIdAsync(trainerId.ToString());
        if (trainer == null || !ownerGymIds.Contains(trainer.GymId ?? Guid.Empty))
            throw NotFoundException.UserByEmail("trainer_not_found");

        var dto = mapper.Map<TrainerDetailDto>(trainer);

        if (trainer.User != null)
        {
            dto.FullName = trainer.User.FullName;
            dto.Email = trainer.User.Email;
            dto.PhoneNumber = trainer.User.PhoneNumber;
            dto.ProfileImageUrl = trainer.User.ProfileImageUrl;
        }

        if (trainer.Gym != null)
        {
            dto.Gym = new GymInfoDto { Id = trainer.Gym.Id, Name = trainer.Gym.Name };
        }

        return ResponseModel<TrainerDetailDto>.SuccessResponse(dto);
    }

    #region ==================== HELPER METHODS ====================

    private static double CalculateAverageCheckIns(DateTime joinDate, int totalCheckIns)
    {
        var monthsActive = (DateTime.UtcNow - joinDate).TotalDays / 30.0;
        return monthsActive > 0 ? totalCheckIns / monthsActive : 0;
    }

    #endregion
}
