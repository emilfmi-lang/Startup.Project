using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Startup_Project.Application.Dtos.Trainer;
using Startup_Project.Application.Interfaces;
using Startup_Project.Application.Models;
using Startup_Project.Application.Services.Interfaces;
using Startup_Project.Domain.Abstractions.Repositories;
using Startup_Project.Domain.Entities;
using Startup_Project.Domain.Enums;
using Startup_Project.Domain.Enums.Errors;
using Startup_Project.Domain.Exceptions;
using System.Globalization;
using System.Security.Claims;

namespace Startup_Project.Application.Services.Concretes;

/// <summary>
/// Trainer Panel — Session Management Service
/// TrainerId hec vaxt frontend-den goturulmur, JWT-den avtomatik alinir
/// </summary>
public class TrainerSessionService(
    IApplicationDbContext context,
    IGenericRepository<Trainer> trainerRepository,
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

    /// <summary>
    /// JWT-den current user-in Trainer profilini tapir
    /// Trainer.UserId == JWT UserId
    /// </summary>
    private async Task<Trainer> GetCurrentTrainerAsync()
    {
        var userId = GetCurrentUserId();

        var trainer = await trainerRepository.Table
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.UserId == userId && !t.IsDeleted);

        if (trainer is null)
            throw NotFoundException.Trainer(userId);

        return trainer;
    }

    private static TrainerSessionResponseDto MapToDto(ScheduledSession session)
    {
        return new TrainerSessionResponseDto
        {
            Id = session.Id,
            Title = session.Title ?? session.ExerciseType,
            Type = session.ExerciseType,
            Description = session.Description,
            Date = session.ScheduledTime.Date,
            StartTime = session.ScheduledTime.ToString("HH:mm"),
            Duration = session.DurationMinutes,
            MaxParticipants = session.MaxParticipants,
            PricePerPerson = session.PricePerPerson,
            Status = session.Status.ToString(),
            TrainerName = session.Trainer?.User?.FullName ?? string.Empty,
            CreatedAt = session.CreatedAt
        };
    }

    #endregion

    #region ==================== GET MY SESSIONS ====================

    public async Task<ResponseModel<List<TrainerSessionResponseDto>>> GetMySessionsAsync()
    {
        var trainer = await GetCurrentTrainerAsync();

        var sessions = await context.ScheduledSessions
            .Include(s => s.Trainer)
                .ThenInclude(t => t.User)
            .Where(s => s.TrainerId == trainer.Id && !s.IsDeleted)
            .OrderByDescending(s => s.ScheduledTime)
            .AsNoTracking()
            .ToListAsync();

        var dtos = sessions.Select(MapToDto).ToList();

        return ResponseModel<List<TrainerSessionResponseDto>>.SuccessResponse(dtos);
    }

    #endregion

    #region ==================== CREATE SESSION ====================

    public async Task<ResponseModel<TrainerSessionResponseDto>> CreateSessionAsync(
        TrainerCreateSessionDto request)
    {
        // FluentValidation
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var trainer = await GetCurrentTrainerAsync();

        if (trainer.GymId is null)
            throw BusinessException.Custom(
                BusinessErrorCodes.TrainerNotAvailable,
                "Online trainer üçün gym-based seans yaratmaq mümkün deyil.");

        // Date + StartTime birlesdirme: "2025-06-20" + "14:30" ? DateTime
        if (!TimeSpan.TryParseExact(request.StartTime, "hh\\:mm", CultureInfo.InvariantCulture, out var time)
            && !TimeSpan.TryParseExact(request.StartTime, "h\\:mm", CultureInfo.InvariantCulture, out time))
        {
            // Regex validator-dan kecib buraya geldise, fallback parse
            time = TimeSpan.Parse(request.StartTime);
        }

        var scheduledTime = request.Date.Date.Add(time);

        if (scheduledTime <= DateTime.UtcNow)
            throw BusinessException.ValidationFailed("Keçmi? zamana seans t?yin etm?k olmaz.");

        // Collision Check — eyni trainer-in eyni vaxtda baska seansi olmamalidir
        var hasCollision = await context.ScheduledSessions
            .Where(s => s.TrainerId == trainer.Id
                && s.Status == ScheduledSessionStatus.Upcoming
                && !s.IsDeleted)
            .AnyAsync(s =>
                s.ScheduledTime < scheduledTime.AddMinutes(request.Duration)
                && s.ScheduledTime.AddMinutes(s.DurationMinutes) > scheduledTime);

        if (hasCollision)
            throw BusinessException.Custom(
                BusinessErrorCodes.SessionTimeCollision,
                "Bu vaxt aral???nda art?q ba?qa seans?n?z var.");

        // Entity yarat
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

        // Response ucun Include ile yeniden yukle
        var created = await context.ScheduledSessions
            .Include(s => s.Trainer)
                .ThenInclude(t => t.User)
            .AsNoTracking()
            .FirstAsync(s => s.Id == session.Id);

        return ResponseModel<TrainerSessionResponseDto>.SuccessResponse(MapToDto(created));
    }

    #endregion
}
