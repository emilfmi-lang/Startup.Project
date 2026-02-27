using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Startup_Project.Application.Dtos.UserSession;
using Startup_Project.Application.Interfaces;
using Startup_Project.Application.Models;
using Startup_Project.Application.Services.Interfaces;
using Startup_Project.Domain.Enums;
using Startup_Project.Domain.Enums.Errors;
using Startup_Project.Domain.Exceptions;
using System.Security.Claims;

namespace Startup_Project.Application.Services.Concretes;

public class UserSessionService(
    IApplicationDbContext context,
    IHttpContextAccessor httpContextAccessor) : IUserSessionService
{
    private Guid GetCurrentUserId()
    {
        var userIdClaim = httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            throw BusinessException.UnauthorizedAccess();

        return userId;
    }

    public async Task<ResponseModel<List<UserSessionDto>>> GetAvailableSessionsAsync(
        string? search = null,
        string? exerciseType = null)
    {
        var userId = GetCurrentUserId();

        // User-?n art?q öd?ni? etdiyi session ID-l?ri
        var bookedSessionIds = await context.SessionPayments
            .Where(p => p.StudentId == userId && p.Status == PaymentStatus.Successful)
            .Select(p => p.ScheduledSessionId)
            .ToListAsync();

        // H?r session üçün u?urlu öd?ni? say? (currentParticipants)
        var participantCounts = await context.SessionPayments
            .Where(p => p.Status == PaymentStatus.Successful)
            .GroupBy(p => p.ScheduledSessionId)
            .Select(g => new { SessionId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.SessionId, x => x.Count);

        var query = context.ScheduledSessions
            .Include(s => s.Trainer).ThenInclude(t => t.User)
            .Include(s => s.Gym)
            .Where(s => !s.IsDeleted
                && s.ScheduledTime > DateTime.UtcNow
                && (s.Status == ScheduledSessionStatus.Pending || s.Status == ScheduledSessionStatus.Confirmed))
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.ToLower();
            query = query.Where(s =>
                (s.Title != null && s.Title.ToLower().Contains(q)) ||
                s.ExerciseType.ToLower().Contains(q) ||
                s.Trainer.User.FullName.ToLower().Contains(q));
        }

        if (!string.IsNullOrWhiteSpace(exerciseType))
        {
            var t = exerciseType.ToLower();
            query = query.Where(s => s.ExerciseType.ToLower().Contains(t));
        }

        var sessions = await query
            .OrderBy(s => s.ScheduledTime)
            .ToListAsync();

        var dtos = sessions.Select(s =>
        {
            var currentParticipants = participantCounts.GetValueOrDefault(s.Id, 0);
            var isBooked = bookedSessionIds.Contains(s.Id);
            var isFull = currentParticipants >= s.MaxParticipants;

            return new UserSessionDto
            {
                Id = s.Id,
                Title = s.Title ?? s.ExerciseType,
                ExerciseType = s.ExerciseType,
                Description = s.Description,
                TrainerId = s.TrainerId,
                TrainerName = s.Trainer.User.FullName,
                Specialization = s.Trainer.Specialization,
                TrainerProfileImage = s.Trainer.ProfileImageUrl,
                TrainerRating = s.Trainer.AverageRating,
                Date = s.ScheduledTime.Date,
                StartTime = s.ScheduledTime.ToString("HH:mm"),
                EndTime = s.EndTime.ToString("HH:mm"),
                DurationMinutes = s.DurationMinutes,
                GymId = s.GymId,
                GymName = s.Gym?.Name ?? "Online",
                IsOnline = s.Trainer.IsOnlineTrainer,
                Price = s.PricePerPerson,
                MaxParticipants = s.MaxParticipants,
                CurrentParticipants = currentParticipants,
                IsFull = isFull,
                Status = s.Status.ToString(),
                CanCancel = false,
                CanBook = !isBooked && !isFull,
                IsBooked = isBooked
            };
        }).ToList();

        return ResponseModel<List<UserSessionDto>>.SuccessResponse(dtos);
    }

    public async Task<ResponseModel<List<UserSessionDto>>> GetMyUpcomingSessionsAsync()
    {
        var userId = GetCurrentUserId();

        var bookedSessionIds = await context.SessionPayments
            .Where(p => p.StudentId == userId && p.Status == PaymentStatus.Successful)
            .Select(p => p.ScheduledSessionId)
            .ToListAsync();

        if (bookedSessionIds.Count == 0)
            return ResponseModel<List<UserSessionDto>>.SuccessResponse([]);

        var sessions = await context.ScheduledSessions
            .Include(s => s.Trainer).ThenInclude(t => t.User)
            .Include(s => s.Gym)
            .Where(s => bookedSessionIds.Contains(s.Id)
                && !s.IsDeleted
                && s.ScheduledTime > DateTime.UtcNow
                && s.Status != ScheduledSessionStatus.Cancelled)
            .OrderBy(s => s.ScheduledTime)
            .AsNoTracking()
            .ToListAsync();

        var dtos = sessions.Select(s => new UserSessionDto
        {
            Id = s.Id,
            Title = s.Title ?? s.ExerciseType,
            ExerciseType = s.ExerciseType,
            Description = s.Description,
            TrainerId = s.TrainerId,
            TrainerName = s.Trainer.User.FullName,
            Specialization = s.Trainer.Specialization,
            TrainerProfileImage = s.Trainer.ProfileImageUrl,
            TrainerRating = s.Trainer.AverageRating,
            Date = s.ScheduledTime.Date,
            StartTime = s.ScheduledTime.ToString("HH:mm"),
            EndTime = s.EndTime.ToString("HH:mm"),
            DurationMinutes = s.DurationMinutes,
            GymId = s.GymId,
            GymName = s.Gym?.Name ?? "Online",
            IsOnline = s.Trainer.IsOnlineTrainer,
            Price = s.PricePerPerson,
            MaxParticipants = s.MaxParticipants,
            CurrentParticipants = 0,
            IsFull = false,
            Status = s.Status.ToString(),
            CanCancel = s.Status != ScheduledSessionStatus.Completed
                && s.ScheduledTime > DateTime.UtcNow.AddHours(2),
            CanBook = false,
            IsBooked = true
        }).ToList();

        return ResponseModel<List<UserSessionDto>>.SuccessResponse(dtos);
    }

    public async Task<ResponseModel<List<UserSessionDto>>> GetMyPastSessionsAsync()
    {
        var userId = GetCurrentUserId();

        var bookedSessionIds = await context.SessionPayments
            .Where(p => p.StudentId == userId && p.Status == PaymentStatus.Successful)
            .Select(p => p.ScheduledSessionId)
            .ToListAsync();

        if (bookedSessionIds.Count == 0)
            return ResponseModel<List<UserSessionDto>>.SuccessResponse([]);

        var sessions = await context.ScheduledSessions
            .Include(s => s.Trainer).ThenInclude(t => t.User)
            .Include(s => s.Gym)
            .Where(s => bookedSessionIds.Contains(s.Id)
                && !s.IsDeleted
                && (s.ScheduledTime <= DateTime.UtcNow || s.Status == ScheduledSessionStatus.Cancelled))
            .OrderByDescending(s => s.ScheduledTime)
            .AsNoTracking()
            .ToListAsync();

        var dtos = sessions.Select(s => new UserSessionDto
        {
            Id = s.Id,
            Title = s.Title ?? s.ExerciseType,
            ExerciseType = s.ExerciseType,
            Description = s.Description,
            TrainerId = s.TrainerId,
            TrainerName = s.Trainer.User.FullName,
            Specialization = s.Trainer.Specialization,
            TrainerProfileImage = s.Trainer.ProfileImageUrl,
            TrainerRating = s.Trainer.AverageRating,
            Date = s.ScheduledTime.Date,
            StartTime = s.ScheduledTime.ToString("HH:mm"),
            EndTime = s.EndTime.ToString("HH:mm"),
            DurationMinutes = s.DurationMinutes,
            GymId = s.GymId,
            GymName = s.Gym?.Name ?? "Online",
            IsOnline = s.Trainer.IsOnlineTrainer,
            Price = s.PricePerPerson,
            MaxParticipants = s.MaxParticipants,
            CurrentParticipants = 0,
            IsFull = false,
            Status = s.Status.ToString(),
            CanCancel = false,
            CanBook = false,
            IsBooked = true
        }).ToList();

        return ResponseModel<List<UserSessionDto>>.SuccessResponse(dtos);
    }

    public async Task<ResponseModel<bool>> CancelBookingAsync(Guid scheduledSessionId)
    {
        var userId = GetCurrentUserId();

        var payment = await context.SessionPayments
            .Include(p => p.ScheduledSession)
            .FirstOrDefaultAsync(p => p.StudentId == userId
                && p.ScheduledSessionId == scheduledSessionId
                && p.Status == PaymentStatus.Successful);

        if (payment is null)
            throw BusinessException.Custom(BusinessErrorCodes.PaymentFailed, "Bu sessiya üçün aktiv qeydiniz yoxdur.");

        var session = payment.ScheduledSession;

        if (session.Status == ScheduledSessionStatus.Completed)
            throw BusinessException.Custom(BusinessErrorCodes.ScheduledSessionInvalidStatus, "Tamamlanm?? sessiya l??v edil? bilm?z.");

        if (session.ScheduledTime <= DateTime.UtcNow.AddHours(2))
            throw BusinessException.Custom(BusinessErrorCodes.ScheduledSessionInvalidStatus, "Sessiyaya 2 saatdan az qald?qda l??v etm?k mümkün deyil.");

        payment.MarkAsRefunded();
        await context.SaveChangesAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }
}
