using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Startup_Project.Application.Dtos.Trainer;
using Startup_Project.Application.Interfaces;
using Startup_Project.Application.Models;
using Startup_Project.Application.Services.Interfaces;
using Startup_Project.Domain.Abstractions.Repositories;
using Startup_Project.Domain.Entities;
using Startup_Project.Domain.Entities.Identity;
using Startup_Project.Domain.Enums;
using Startup_Project.Domain.Enums.Errors;
using Startup_Project.Domain.Exceptions;
using System.Globalization;
using System.Security.Claims;

namespace Startup_Project.Application.Services.Concretes;

public class TrainerSessionService(
    IApplicationDbContext context,
    IGenericRepository<Trainer> trainerRepository,
    UserManager<AppUser> userManager,
    IValidator<TrainerCreateSessionDto> validator,
    IHttpContextAccessor httpContextAccessor) : ITrainerSessionService
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
    private async Task<Trainer?> FindCurrentTrainerAsync()
    {
        var userId = GetCurrentUserId();

        return await trainerRepository.Table
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.UserId == userId && !t.IsDeleted);
    }

    private async Task<Trainer> RequireCurrentTrainerAsync()
    {
        var trainer = await FindCurrentTrainerAsync();
        if (trainer is not null)
            return trainer;

        // Trainer row yoxdur — avtomatik profil yarat
        var userId = GetCurrentUserId();
        var user = await userManager.FindByIdAsync(userId.ToString())
            ?? throw BusinessException.UnauthorizedAccess();

        // Userin gym-ini tap
        Guid? gymId = user.GymId;

        // Eger userin GymId-si yoxdursa, her hansi aktiv gymu yoxla
        if (!gymId.HasValue)
        {
            var ownerGym = await context.Gyms
                .Where(g => g.OwnerId == userId && !g.IsDeleted)
                .Select(g => g.Id)
                .FirstOrDefaultAsync();

            if (ownerGym != default)
                gymId = ownerGym;
        }

        // Gym-based trainer yaradilir (gymId null ola biler — online trainer)
        Trainer newTrainer;
        if (gymId.HasValue)
        {
            newTrainer = new Trainer(userId, gymId.Value, "General", 0);
        }
        else
        {
            newTrainer = new Trainer(userId, "General", 0);
        }

        await trainerRepository.AddAsync(newTrainer);
        await trainerRepository.SaveAsync();

        // Yeniden Include ile yukle
        return await trainerRepository.Table
            .Include(t => t.User)
            .FirstAsync(t => t.Id == newTrainer.Id);
    }

    private static TrainerSessionResponseDto MapToDto(ScheduledSession s) => new()
    {
        Id = s.Id,
        Title = s.Title ?? s.ExerciseType,
        Type = s.ExerciseType,
        Description = s.Description,
        Date = s.ScheduledTime.Date,
        StartTime = s.ScheduledTime.ToString("HH:mm"),
        EndTime = s.EndTime.ToString("HH:mm"),
        Duration = s.DurationMinutes,
        MaxParticipants = s.MaxParticipants,
        PricePerPerson = s.PricePerPerson,
        Status = s.Status.ToString().ToLowerInvariant(),
        TrainerName = s.Trainer?.User?.FullName ?? string.Empty,
        MemberName = s.Member?.FullName,
        CreatedAt = s.CreatedAt
    };

    #endregion

    #region ==================== GET MY SESSIONS ====================

    public async Task<ResponseModel<List<TrainerSessionResponseDto>>> GetMySessionsAsync(
        string? search = null,
        string? status = null)
    {
        var trainer = await FindCurrentTrainerAsync();

        // Trainer profili yoxdursa — bos siyahi qaytar, xeta atma
        if (trainer is null)
            return ResponseModel<List<TrainerSessionResponseDto>>.SuccessResponse([]);

        var query = context.ScheduledSessions
            .Include(s => s.Trainer).ThenInclude(t => t.User)
            .Include(s => s.Member)
            .Where(s => s.TrainerId == trainer.Id && !s.IsDeleted)
            .AsNoTracking();

        // Search: title, type, member name
        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.ToLower();
            query = query.Where(s =>
                (s.Title != null && s.Title.ToLower().Contains(q)) ||
                s.ExerciseType.ToLower().Contains(q) ||
                (s.Member != null && s.Member.FullName.ToLower().Contains(q)));
        }

        // Status filter
        if (!string.IsNullOrWhiteSpace(status) && status.ToLower() != "all")
        {
            if (Enum.TryParse<ScheduledSessionStatus>(status, ignoreCase: true, out var parsed))
                query = query.Where(s => s.Status == parsed);
        }

        var sessions = await query
            .OrderByDescending(s => s.ScheduledTime)
            .ToListAsync();

        return ResponseModel<List<TrainerSessionResponseDto>>.SuccessResponse(
            sessions.Select(MapToDto).ToList());
    }

    #endregion

    #region ==================== CREATE SESSION ====================

    public async Task<ResponseModel<TrainerSessionResponseDto>> CreateSessionAsync(
        TrainerCreateSessionDto request)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var trainer = await RequireCurrentTrainerAsync();

        if (trainer.GymId is null)
            throw BusinessException.Custom(
                BusinessErrorCodes.TrainerNotAvailable,
                "Online trainer ucun gym-based seans yaratmaq mumkun deyil.");

        // Date + StartTime -> DateTime
        var time = TimeSpan.ParseExact(request.StartTime, ["hh\\:mm", "h\\:mm"], CultureInfo.InvariantCulture);
        var scheduledTime = request.Date.Date.Add(time);

        if (scheduledTime <= DateTime.UtcNow)
            throw BusinessException.ValidationFailed("Kecmis zamana seans teyin etmek olmaz.");

        // Collision Check
        var hasCollision = await context.ScheduledSessions
            .Where(s => s.TrainerId == trainer.Id
                && (s.Status == ScheduledSessionStatus.Pending || s.Status == ScheduledSessionStatus.Confirmed)
                && !s.IsDeleted)
            .AnyAsync(s =>
                s.ScheduledTime < scheduledTime.AddMinutes(request.Duration)
                && s.ScheduledTime.AddMinutes(s.DurationMinutes) > scheduledTime);

        if (hasCollision)
            throw BusinessException.Custom(
                BusinessErrorCodes.SessionTimeCollision,
                "Bu vaxt araliginda artiq basqa seansiniz var.");

        var session = new ScheduledSession(
            trainerId: trainer.Id,
            gymId: trainer.GymId.Value,
            title: request.Title,
            exerciseType: request.Type,
            description: request.Description,
            scheduledTime: scheduledTime,
            durationMinutes: request.Duration,
            maxParticipants: request.MaxParticipants,
            pricePerPerson: request.PricePerPerson);

        context.ScheduledSessions.Add(session);
        await context.SaveChangesAsync();

        var created = await context.ScheduledSessions
            .Include(s => s.Trainer).ThenInclude(t => t.User)
            .Include(s => s.Member)
            .AsNoTracking()
            .FirstAsync(s => s.Id == session.Id);

        return ResponseModel<TrainerSessionResponseDto>.SuccessResponse(MapToDto(created));
    }

    #endregion

    #region ==================== UPDATE STATUS ====================

    public async Task<ResponseModel<TrainerSessionResponseDto>> UpdateSessionStatusAsync(
        Guid sessionId,
        string status)
    {
        var trainer = await RequireCurrentTrainerAsync();

        var session = await context.ScheduledSessions
            .Include(s => s.Trainer).ThenInclude(t => t.User)
            .Include(s => s.Member)
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.TrainerId == trainer.Id && !s.IsDeleted);

        if (session is null)
            throw BusinessException.Custom(BusinessErrorCodes.ScheduledSessionNotFound, "Seans tapilmadi.");

        if (!Enum.TryParse<ScheduledSessionStatus>(status, ignoreCase: true, out var newStatus))
            throw BusinessException.Custom(BusinessErrorCodes.ScheduledSessionInvalidStatus, "Kecersiz status.");

        switch (newStatus)
        {
            case ScheduledSessionStatus.Confirmed:
                session.Confirm();
                break;
            case ScheduledSessionStatus.Completed:
                session.MarkAsCompleted();
                break;
            case ScheduledSessionStatus.Cancelled:
                session.Cancel();
                break;
            default:
                throw BusinessException.Custom(
                    BusinessErrorCodes.ScheduledSessionInvalidStatus,
                    "Yalniz confirmed, completed ve ya cancelled statusuna deyismek mumkundur.");
        }

        await context.SaveChangesAsync();

        return ResponseModel<TrainerSessionResponseDto>.SuccessResponse(MapToDto(session));
    }

    #endregion
}
