using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Startup_Project.Application.Dtos.Admin;
using Startup_Project.Application.Interfaces;
using Startup_Project.Application.Models;
using Startup_Project.Application.Services.Interfaces.Admin;
using Startup_Project.Domain.Abstractions.Repositories;
using Startup_Project.Domain.Entities;
using Startup_Project.Domain.Entities.Identity;
using Startup_Project.Domain.Enums;
using Startup_Project.Domain.Enums.Errors;
using Startup_Project.Domain.Exceptions;
using System.Security.Claims;

namespace Startup_Project.Application.Services.Concretes.Admin;

public class GymAdminSessionService(
    IApplicationDbContext context,
    IGenericRepository<Trainer> trainerRepository,
    UserManager<AppUser> userManager,
    IValidator<CreateSessionDto> sessionValidator,
    IHttpContextAccessor httpContextAccessor) : IGymAdminSessionService
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

    private async Task<Gym> GetAdminGymAsync()
    {
        var ownerId = GetCurrentUserId();

        var gym = await context.Gyms
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.OwnerId == ownerId && !g.IsDeleted);

        if (gym is null)
            throw NotFoundException.GymForOwner();

        return gym;
    }

    private static SessionResponseDto MapToDto(ScheduledSession session)
    {
        return new SessionResponseDto
        {
            Id = session.Id,
            TrainerId = session.TrainerId,
            TrainerName = session.Trainer?.User?.FullName ?? string.Empty,
            MemberId = session.MemberId,
            MemberName = session.Member?.FullName ?? string.Empty,
            ExerciseType = session.ExerciseType,
            ScheduledTime = session.ScheduledTime,
            DurationMinutes = session.DurationMinutes,
            Status = session.Status.ToString(),
            CreatedAt = session.CreatedAt
        };
    }

    #endregion

    #region ==================== GET TRAINER SESSIONS ====================

    public async Task<ResponseModel<List<SessionResponseDto>>> GetTrainerSessionsAsync(Guid trainerId)
    {
        var gym = await GetAdminGymAsync();

        // Trainer-in bu zala aid oldugunu yoxla
        var trainerExists = await trainerRepository.Table
            .AnyAsync(t => t.Id == trainerId && t.GymId == gym.Id && !t.IsDeleted);

        if (!trainerExists)
            throw BusinessException.Custom(BusinessErrorCodes.TrainerNotFound, "M??qçi bu zala aid deyil.");

        var sessions = await context.ScheduledSessions
            .Include(s => s.Trainer)
                .ThenInclude(t => t.User)
            .Include(s => s.Member)
            .Where(s => s.TrainerId == trainerId && s.GymId == gym.Id && !s.IsDeleted)
            .OrderByDescending(s => s.ScheduledTime)
            .AsNoTracking()
            .ToListAsync();

        var dtos = sessions.Select(MapToDto).ToList();

        return ResponseModel<List<SessionResponseDto>>.SuccessResponse(dtos);
    }

    #endregion

    #region ==================== CREATE SESSION ====================

    public async Task<ResponseModel<SessionResponseDto>> CreateSessionAsync(CreateSessionDto request)
    {
        // FluentValidation
        var validationResult = await sessionValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var gym = await GetAdminGymAsync();

        // Trainer yoxla — bu zala aid olmalidir
        var trainer = await trainerRepository.Table
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == request.TrainerId && t.GymId == gym.Id && !t.IsDeleted);

        if (trainer is null)
            throw BusinessException.Custom(BusinessErrorCodes.TrainerNotFound, "M??qçi bu zala aid deyil.");

        // Member yoxla — bu zala aid olmalidir
        var member = await userManager.Users
            .FirstOrDefaultAsync(u => u.Id == request.MemberId && u.GymId == gym.Id);

        if (member is null)
            throw BusinessException.Custom(BusinessErrorCodes.UserNotFound, "Üzv bu zala aid deyil.");

        // Subscription Check — memberin aktiv abuneliyinin olub-olmadigini yoxla
        var gymPackageIds = await context.Packages
            .Where(p => p.GymId == gym.Id && !p.IsDeleted)
            .Select(p => p.Id)
            .ToListAsync();

        var hasActiveSubscription = await context.Subscriptions
            .AnyAsync(s => s.UserId == request.MemberId
                && gymPackageIds.Contains(s.PackageId)
                && s.EndDate > DateTime.UtcNow);

        if (!hasActiveSubscription)
            throw BusinessException.Custom(
                BusinessErrorCodes.MemberSubscriptionExpired,
                "Üzvün aktiv abun?liyi yoxdur. Seans t?yin etm?k üçün aktiv abun?lik laz?md?r.");

        // Collision Check — eyni trainer-in eyni vaxtda baska seansi olmamalidir
        var hasCollision = await context.ScheduledSessions
            .Where(s => s.TrainerId == request.TrainerId
                && s.Status == ScheduledSessionStatus.Upcoming
                && !s.IsDeleted)
            .AnyAsync(s =>
                s.ScheduledTime < request.ScheduledTime.AddMinutes(request.DurationMinutes)
                && s.ScheduledTime.AddMinutes(s.DurationMinutes) > request.ScheduledTime);

        if (hasCollision)
            throw BusinessException.Custom(
                BusinessErrorCodes.SessionTimeCollision,
                "M??qçinin bu vaxt aral???nda ba?qa seans? var.");

        // Yarat
        var session = new ScheduledSession(
            request.TrainerId,
            request.MemberId,
            gym.Id,
            request.ExerciseType,
            request.ScheduledTime,
            request.DurationMinutes);

        context.ScheduledSessions.Add(session);
        await context.SaveChangesAsync();

        // Response ucun Include ile yeniden yukle
        var created = await context.ScheduledSessions
            .Include(s => s.Trainer)
                .ThenInclude(t => t.User)
            .Include(s => s.Member)
            .AsNoTracking()
            .FirstAsync(s => s.Id == session.Id);

        return ResponseModel<SessionResponseDto>.SuccessResponse(MapToDto(created));
    }

    #endregion

    #region ==================== UPDATE SESSION STATUS ====================

    public async Task<ResponseModel<SessionResponseDto>> UpdateSessionStatusAsync(
        Guid sessionId,
        UpdateSessionStatusDto request)
    {
        var gym = await GetAdminGymAsync();

        var session = await context.ScheduledSessions
            .Include(s => s.Trainer)
                .ThenInclude(t => t.User)
            .Include(s => s.Member)
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.GymId == gym.Id && !s.IsDeleted);

        if (session is null)
            throw BusinessException.Custom(
                BusinessErrorCodes.ScheduledSessionNotFound,
                "Seans tap?lmad?.");

        switch (request.Status)
        {
            case ScheduledSessionStatus.Completed:
                session.MarkAsCompleted();
                break;
            case ScheduledSessionStatus.Cancelled:
                session.Cancel();
                break;
            default:
                throw BusinessException.Custom(
                    BusinessErrorCodes.ScheduledSessionInvalidStatus,
                    "Yaln?z Completed v? ya Cancelled statusuna d?yi?m?k mümkündür.");
        }

        await context.SaveChangesAsync();

        return ResponseModel<SessionResponseDto>.SuccessResponse(MapToDto(session));
    }

    #endregion
}
