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
using System.Security.Claims;

namespace Startup_Project.Application.Services.Concretes;

public class TrainerStudentService(
    IApplicationDbContext context,
    IGenericRepository<Trainer> trainerRepository,
    UserManager<AppUser> userManager,
    IHttpContextAccessor httpContextAccessor) : ITrainerStudentService
{
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

    #region ==================== GET MY STUDENTS ====================

    public async Task<ResponseModel<TrainerStudentsPageDto>> GetMyStudentsAsync(
        string? search = null,
        string? status = null)
    {
        var trainer = await FindCurrentTrainerAsync();

        if (trainer is null)
            return ResponseModel<TrainerStudentsPageDto>.SuccessResponse(new TrainerStudentsPageDto());

        // Yalniz TrainerStudents cedvelinden — bu trainere aid olan telebeler
        var trainerStudents = await context.TrainerStudents
            .Include(ts => ts.Student)
            .Where(ts => ts.TrainerId == trainer.Id && !ts.IsDeleted)
            .AsNoTracking()
            .ToListAsync();

        if (trainerStudents.Count == 0)
            return ResponseModel<TrainerStudentsPageDto>.SuccessResponse(new TrainerStudentsPageDto());

        var studentIds = trainerStudents.Select(ts => ts.StudentId).ToList();

        // Bu trainerin bu telebelerle olan seanslari
        var sessions = await context.ScheduledSessions
            .Where(s => s.TrainerId == trainer.Id
                && s.MemberId != null
                && studentIds.Contains(s.MemberId.Value)
                && !s.IsDeleted)
            .AsNoTracking()
            .ToListAsync();

        // Aktiv abunelikleri yoxla
        var gymPackageIds = trainer.GymId.HasValue
            ? await context.Packages
                .Where(p => p.GymId == trainer.GymId.Value && !p.IsDeleted)
                .Select(p => p.Id)
                .ToListAsync()
            : [];

        var activeSubUserIds = gymPackageIds.Count > 0
            ? await context.Subscriptions
                .Where(s => studentIds.Contains(s.UserId)
                    && gymPackageIds.Contains(s.PackageId)
                    && s.EndDate > DateTime.UtcNow)
                .Select(s => s.UserId)
                .Distinct()
                .ToListAsync()
            : [];

        // DTO-lari yarat
        var today = DateTime.UtcNow.Date;
        var studentDtos = new List<TrainerStudentDto>();

        foreach (var ts in trainerStudents)
        {
            var studentSessions = sessions
                .Where(s => s.MemberId == ts.StudentId)
                .ToList();

            var completedCount = studentSessions
                .Count(s => s.Status == ScheduledSessionStatus.Completed);

            var totalFromEntity = ts.TotalSessions;
            var progressPercent = totalFromEntity > 0
                ? (int)Math.Round((double)completedCount / totalFromEntity * 100)
                : 0;

            if (progressPercent > 100) progressPercent = 100;

            var nextSession = studentSessions
                .Where(s => s.ScheduledTime > DateTime.UtcNow
                    && s.Status is ScheduledSessionStatus.Pending or ScheduledSessionStatus.Confirmed)
                .OrderBy(s => s.ScheduledTime)
                .FirstOrDefault();

            var isActive = activeSubUserIds.Contains(ts.StudentId);

            studentDtos.Add(new TrainerStudentDto
            {
                Id = ts.StudentId,
                FullName = ts.Student.FullName,
                ProfileImageUrl = ts.Student.ProfileImageUrl,
                Program = ts.Program,
                CompletedSessions = completedCount,
                TotalSessions = totalFromEntity,
                ProgressPercent = progressPercent,
                Status = isActive ? "active" : "inactive",
                NextSessionDate = nextSession?.ScheduledTime.Date
            });
        }

        // Search
        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.ToLower();
            studentDtos = studentDtos
                .Where(s => s.FullName.ToLower().Contains(q)
                    || s.Program.ToLower().Contains(q))
                .ToList();
        }

        // Status filter
        if (!string.IsNullOrWhiteSpace(status) && !status.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            studentDtos = studentDtos
                .Where(s => s.Status.Equals(status, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        // Stats
        var sessionsToday = sessions
            .Count(s => s.ScheduledTime.Date == today
                && s.Status is ScheduledSessionStatus.Pending or ScheduledSessionStatus.Confirmed);

        var activeNow = sessions
            .Count(s => s.ScheduledTime <= DateTime.UtcNow
                && s.ScheduledTime.AddMinutes(s.DurationMinutes) >= DateTime.UtcNow
                && s.Status is ScheduledSessionStatus.Pending or ScheduledSessionStatus.Confirmed);

        var avgProgress = studentDtos.Count > 0
            ? (int)Math.Round(studentDtos.Average(s => s.ProgressPercent))
            : 0;

        return ResponseModel<TrainerStudentsPageDto>.SuccessResponse(new TrainerStudentsPageDto
        {
            TotalStudents = studentDtos.Count,
            AvgProgress = avgProgress,
            SessionsToday = sessionsToday,
            ActiveNow = activeNow,
            Students = studentDtos
                .OrderByDescending(s => s.Status == "active")
                .ThenBy(s => s.FullName)
                .ToList()
        });
    }

    #endregion

    #region ==================== ADD STUDENT ====================

    public async Task<ResponseModel<TrainerStudentDto>> AddStudentAsync(AddStudentDto request)
    {
        var trainer = await FindCurrentTrainerAsync()
            ?? throw BusinessException.Custom(BusinessErrorCodes.TrainerNotFound,
                "Trainer profiliniz tap?lmad?.");

        // Student movcuddur?
        var student = await userManager.FindByIdAsync(request.StudentId.ToString())
            ?? throw BusinessException.Custom(BusinessErrorCodes.UserNotFound,
                "?stifad?çi tap?lmad?.");

        // Artiq elave olunub?
        var exists = await context.TrainerStudents
            .AnyAsync(ts => ts.TrainerId == trainer.Id
                && ts.StudentId == request.StudentId
                && !ts.IsDeleted);

        if (exists)
            throw BusinessException.Custom(BusinessErrorCodes.StudentAlreadyAssigned,
                "Bu t?l?b? art?q sizin siyah?n?zdad?r.");

        var trainerStudent = new TrainerStudent(
            trainer.Id,
            request.StudentId,
            request.Program,
            request.TotalSessions);

        context.TrainerStudents.Add(trainerStudent);
        await context.SaveChangesAsync();

        return ResponseModel<TrainerStudentDto>.SuccessResponse(new TrainerStudentDto
        {
            Id = student.Id,
            FullName = student.FullName,
            ProfileImageUrl = student.ProfileImageUrl,
            Program = request.Program,
            CompletedSessions = 0,
            TotalSessions = request.TotalSessions,
            ProgressPercent = 0,
            Status = "active",
            NextSessionDate = null
        });
    }

    #endregion

    #region ==================== REMOVE STUDENT ====================

    public async Task<ResponseModel<bool>> RemoveStudentAsync(Guid studentId)
    {
        var trainer = await FindCurrentTrainerAsync()
            ?? throw BusinessException.Custom(BusinessErrorCodes.TrainerNotFound,
                "Trainer profiliniz tap?lmad?.");

        var trainerStudent = await context.TrainerStudents
            .FirstOrDefaultAsync(ts => ts.TrainerId == trainer.Id
                && ts.StudentId == studentId
                && !ts.IsDeleted);

        if (trainerStudent is null)
            throw BusinessException.Custom(BusinessErrorCodes.StudentNotFound,
                "T?l?b? siyah?n?zda tap?lmad?.");

        trainerStudent.IsDeleted = true;
        await context.SaveChangesAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }

    #endregion
}
