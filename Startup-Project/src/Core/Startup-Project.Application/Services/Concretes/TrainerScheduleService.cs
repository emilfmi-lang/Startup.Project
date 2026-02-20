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

public class TrainerScheduleService(
    IApplicationDbContext context,
    IGenericRepository<Trainer> trainerRepository,
    UserManager<AppUser> userManager,
    IHttpContextAccessor httpContextAccessor) : ITrainerScheduleService
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

        var userId = GetCurrentUserId();
        var user = await userManager.FindByIdAsync(userId.ToString())
            ?? throw BusinessException.UnauthorizedAccess();

        Guid? gymId = user.GymId;
        if (!gymId.HasValue)
        {
            var ownerGym = await context.Gyms
                .Where(g => g.OwnerId == userId && !g.IsDeleted)
                .Select(g => g.Id)
                .FirstOrDefaultAsync();
            if (ownerGym != default) gymId = ownerGym;
        }

        var newTrainer = gymId.HasValue
            ? new Trainer(userId, gymId.Value, "General", 0)
            : new Trainer(userId, "General", 0);

        await trainerRepository.AddAsync(newTrainer);
        await trainerRepository.SaveAsync();

        return await trainerRepository.Table
            .Include(t => t.User)
            .FirstAsync(t => t.Id == newTrainer.Id);
    }

    private static DateTime GetWeekStart(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.Date.AddDays(-diff);
    }

    private static ScheduleSessionItemDto MapToItem(ScheduledSession s) => new()
    {
        Id = s.Id,
        Title = s.Title ?? s.ExerciseType,
        Type = s.ExerciseType,
        Description = s.Description,
        StartTime = s.ScheduledTime.ToString("HH:mm"),
        EndTime = s.EndTime.ToString("HH:mm"),
        Duration = s.DurationMinutes,
        MemberName = s.Member?.FullName,
        Status = s.Status.ToString().ToLowerInvariant()
    };

    private static DateTime ParseScheduledTime(DateTime date, string startTime)
    {
        var time = TimeSpan.ParseExact(startTime, [@"hh\:mm", @"h\:mm"], CultureInfo.InvariantCulture);
        return date.Date.Add(time);
    }

    #endregion

    #region ==================== GET WEEKLY SCHEDULE ====================

    public async Task<ResponseModel<WeeklyScheduleDto>> GetWeeklyScheduleAsync(DateTime? weekStart = null)
    {
        var trainer = await FindCurrentTrainerAsync();

        var mondayStart = weekStart.HasValue
            ? GetWeekStart(weekStart.Value)
            : GetWeekStart(DateTime.UtcNow);

        var sundayEnd = mondayStart.AddDays(7);
        var today = DateTime.UtcNow.Date;

        List<ScheduledSession> sessions = [];

        if (trainer is not null)
        {
            sessions = await context.ScheduledSessions
                .Include(s => s.Member)
                .Where(s => s.TrainerId == trainer.Id
                    && !s.IsDeleted
                    && s.ScheduledTime >= mondayStart
                    && s.ScheduledTime < sundayEnd)
                .OrderBy(s => s.ScheduledTime)
                .AsNoTracking()
                .ToListAsync();
        }

        var days = new List<ScheduleDayDto>();
        for (var i = 0; i < 7; i++)
        {
            var date = mondayStart.AddDays(i);
            var daySessions = sessions
                .Where(s => s.ScheduledTime.Date == date.Date)
                .Select(MapToItem)
                .ToList();

            days.Add(new ScheduleDayDto
            {
                Date = date,
                DayName = date.DayOfWeek.ToString()[..3].ToUpperInvariant(),
                DayNumber = date.Day,
                IsToday = date.Date == today,
                Sessions = daySessions
            });
        }

        var weekLabel = $"{mondayStart:MMMM d} - {sundayEnd.AddDays(-1):MMMM d, yyyy}";

        return ResponseModel<WeeklyScheduleDto>.SuccessResponse(new WeeklyScheduleDto
        {
            WeekStart = mondayStart,
            WeekEnd = sundayEnd.AddDays(-1),
            WeekLabel = weekLabel,
            Days = days
        });
    }

    #endregion

    #region ==================== GET TODAY SCHEDULE ====================

    public async Task<ResponseModel<TodayScheduleDto>> GetTodayScheduleAsync()
    {
        var trainer = await FindCurrentTrainerAsync();
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        List<ScheduledSession> sessions = [];

        if (trainer is not null)
        {
            sessions = await context.ScheduledSessions
                .Include(s => s.Member)
                .Where(s => s.TrainerId == trainer.Id
                    && !s.IsDeleted
                    && s.ScheduledTime >= today
                    && s.ScheduledTime < tomorrow
                    && s.Status != ScheduledSessionStatus.Cancelled)
                .OrderBy(s => s.ScheduledTime)
                .AsNoTracking()
                .ToListAsync();
        }

        return ResponseModel<TodayScheduleDto>.SuccessResponse(new TodayScheduleDto
        {
            Date = today,
            TotalSessions = sessions.Count,
            Sessions = sessions.Select(MapToItem).ToList()
        });
    }

    #endregion

    #region ==================== CREATE SCHEDULE ENTRY ====================

    public async Task<ResponseModel<ScheduleSessionItemDto>> CreateScheduleEntryAsync(CreateScheduleEntryDto request)
    {
        var trainer = await RequireCurrentTrainerAsync();

        if (trainer.GymId is null)
            throw BusinessException.Custom(
                BusinessErrorCodes.TrainerNotAvailable,
                "Online trainer ucun gym-based schedule yaratmaq mumkun deyil.");

        var scheduledTime = ParseScheduledTime(request.Date, request.StartTime);

        if (scheduledTime <= DateTime.UtcNow)
            throw BusinessException.ValidationFailed("Kecmis zamana schedule teyin etmek olmaz.");

        // Collision check
        var hasCollision = await context.ScheduledSessions
            .Where(s => s.TrainerId == trainer.Id
                && s.Status != ScheduledSessionStatus.Cancelled
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

        return ResponseModel<ScheduleSessionItemDto>.SuccessResponse(MapToItem(session));
    }

    #endregion

    #region ==================== UPDATE SCHEDULE ENTRY ====================

    public async Task<ResponseModel<ScheduleSessionItemDto>> UpdateScheduleEntryAsync(
        Guid id, UpdateScheduleEntryDto request)
    {
        var trainer = await RequireCurrentTrainerAsync();

        var session = await context.ScheduledSessions
            .Include(s => s.Member)
            .FirstOrDefaultAsync(s => s.Id == id && s.TrainerId == trainer.Id && !s.IsDeleted);

        if (session is null)
            throw BusinessException.Custom(BusinessErrorCodes.ScheduledSessionNotFound, "Schedule tapilmadi.");

        if (session.Status == ScheduledSessionStatus.Completed)
            throw BusinessException.ValidationFailed("Tamamlanmis seansi redakte etmek olmaz.");

        var scheduledTime = ParseScheduledTime(request.Date, request.StartTime);

        // Collision check (ozunu cixart)
        var hasCollision = await context.ScheduledSessions
            .Where(s => s.TrainerId == trainer.Id
                && s.Id != id
                && s.Status != ScheduledSessionStatus.Cancelled
                && !s.IsDeleted)
            .AnyAsync(s =>
                s.ScheduledTime < scheduledTime.AddMinutes(request.Duration)
                && s.ScheduledTime.AddMinutes(s.DurationMinutes) > scheduledTime);

        if (hasCollision)
            throw BusinessException.Custom(
                BusinessErrorCodes.SessionTimeCollision,
                "Bu vaxt araliginda artiq basqa seansiniz var.");

        session.UpdateSchedule(
            request.Title,
            request.Type,
            request.Description,
            scheduledTime,
            request.Duration,
            request.MaxParticipants,
            request.PricePerPerson);

        await context.SaveChangesAsync();

        return ResponseModel<ScheduleSessionItemDto>.SuccessResponse(MapToItem(session));
    }

    #endregion

    #region ==================== DELETE SCHEDULE ENTRY ====================

    public async Task<ResponseModel<bool>> DeleteScheduleEntryAsync(Guid id)
    {
        var trainer = await RequireCurrentTrainerAsync();

        var session = await context.ScheduledSessions
            .FirstOrDefaultAsync(s => s.Id == id && s.TrainerId == trainer.Id && !s.IsDeleted);

        if (session is null)
            throw BusinessException.Custom(BusinessErrorCodes.ScheduledSessionNotFound, "Schedule tapilmadi.");

        if (session.Status == ScheduledSessionStatus.Completed)
            throw BusinessException.ValidationFailed("Tamamlanmis seansi silmek olmaz.");

        session.IsDeleted = true;
        await context.SaveChangesAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }

    #endregion

    #region ==================== BULK UPDATE WEEK ====================

    public async Task<ResponseModel<WeeklyScheduleDto>> BulkUpdateWeekAsync(BulkUpdateScheduleDto request)
    {
        var trainer = await RequireCurrentTrainerAsync();

        if (trainer.GymId is null)
            throw BusinessException.Custom(
                BusinessErrorCodes.TrainerNotAvailable,
                "Online trainer ucun schedule yaratmaq mumkun deyil.");

        var mondayStart = GetWeekStart(request.WeekStart);
        var sundayEnd = mondayStart.AddDays(7);

        // Hefte erzindeki movcud pending/confirmed sessionlari sil
        var existingSessions = await context.ScheduledSessions
            .Where(s => s.TrainerId == trainer.Id
                && !s.IsDeleted
                && s.ScheduledTime >= mondayStart
                && s.ScheduledTime < sundayEnd
                && s.Status != ScheduledSessionStatus.Completed
                && s.MemberId == null) // yalniz member-e bagli olmayanlari sil
            .ToListAsync();

        foreach (var s in existingSessions)
            s.IsDeleted = true;

        // Yeni entry-leri yarat
        foreach (var entry in request.Entries)
        {
            var scheduledTime = ParseScheduledTime(entry.Date, entry.StartTime);

            if (scheduledTime < mondayStart || scheduledTime >= sundayEnd)
                continue; // heftenin xaricindeki entry-leri ignore et

            var session = new ScheduledSession(
                trainerId: trainer.Id,
                gymId: trainer.GymId.Value,
                title: entry.Title,
                exerciseType: entry.Type,
                description: entry.Description,
                scheduledTime: scheduledTime,
                durationMinutes: entry.Duration,
                maxParticipants: entry.MaxParticipants,
                pricePerPerson: entry.PricePerPerson);

            context.ScheduledSessions.Add(session);
        }

        await context.SaveChangesAsync();

        return await GetWeeklyScheduleAsync(mondayStart);
    }

    #endregion
}
