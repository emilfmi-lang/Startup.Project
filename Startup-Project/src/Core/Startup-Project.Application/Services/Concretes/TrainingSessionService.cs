using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Startup_Project.Application.Dtos.TrainingSession;
using Startup_Project.Application.Models;
using Startup_Project.Application.Services.Interfaces;
using Startup_Project.Domain.Abstractions.Repositories;
using Startup_Project.Domain.Entities;
using Startup_Project.Domain.Enums;
using Startup_Project.Domain.Enums.Errors;
using Startup_Project.Domain.Exceptions;
using System.Security.Claims;

namespace Startup_Project.Application.Services.Concretes;

public class TrainingSessionService(
    IGenericRepository<TrainingSession> sessionRepository,
    IGenericRepository<Trainer> trainerRepository,
    IGenericRepository<Gym> gymRepository,
    IHttpContextAccessor httpContextAccessor,
    IMapper mapper,
    IValidator<SessionPurchaseDto> purchaseValidator,
    IValidator<SessionCancelDto> cancelValidator,
    IValidator<SessionVerifyDto> verifyValidator) : ITrainingSessionService
{
    private Guid GetCurrentUserId()
    {
        var userIdClaim = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            throw BusinessException.UnauthorizedAccess();
        return userId;
    }

    private async Task<Guid?> GetCurrentUserGymIdAsync()
    {
        var userId = GetCurrentUserId();
        var gym = await gymRepository.Table
            .FirstOrDefaultAsync(g => g.OwnerId == userId && !g.IsDeleted);
        return gym?.Id;
    }

    private async Task<Trainer> GetCurrentTrainerAsync()
    {
        var userId = GetCurrentUserId();
        var trainer = await trainerRepository.Table
            .FirstOrDefaultAsync(t => t.UserId == userId && !t.IsDeleted);
        if (trainer == null)
            throw NotFoundException.Trainer(userId);
        return trainer;
    }

    // ????????????????????????????????????????????????????????????????
    // USER ENDPOINTS
    // ????????????????????????????????????????????????????????????????

    public async Task<ResponseModel<string>> PurchaseSessionAsync(SessionPurchaseDto dto)
    {
        var validationResult = await purchaseValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            throw BusinessException.ValidationFailed(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

        var userId = GetCurrentUserId();

        var trainer = await trainerRepository.Table
            .Include(t => t.Gym)
            .FirstOrDefaultAsync(t => t.Id == dto.TrainerId && !t.IsDeleted);

        if (trainer == null)
            throw NotFoundException.Trainer(dto.TrainerId);

        if (trainer.Status != TrainerStatus.Active)
            throw BusinessException.TrainerNotAvailable();

        if (trainer.IsOnlineTrainer && !trainer.IsVerified)
            throw BusinessException.Custom(BusinessErrorCodes.TrainerNotVerified, "Bu online m??qçi h?l? t?sdiql?nm?yib.");

        var session = new TrainingSession(
            trainerId: trainer.Id,
            userId: userId,
            gymId: trainer.GymId ?? Guid.Empty,
            price: trainer.SessionPrice
        );

        await sessionRepository.AddAsync(session);
        await sessionRepository.SaveAsync();

        return ResponseModel<string>.SuccessResponse(session.Id.ToString());
    }

    public async Task<ResponseModel<List<SessionDto>>> GetMySessionsAsync()
    {
        var userId = GetCurrentUserId();

        var sessions = await sessionRepository.Table
            .Include(s => s.Trainer)
                .ThenInclude(t => t.User)
            .Include(s => s.Gym)
            .Where(s => s.UserId == userId && !s.IsDeleted)
            .OrderByDescending(s => s.CreatedAt)
            .AsNoTracking()
            .ToListAsync();

        var dtos = mapper.Map<List<SessionDto>>(sessions);
        return ResponseModel<List<SessionDto>>.SuccessResponse(dtos);
    }

    public async Task<ResponseModel<SessionDto>> GetSessionByIdAsync(Guid sessionId)
    {
        var userId = GetCurrentUserId();

        var session = await sessionRepository.Table
            .Include(s => s.Trainer)
                .ThenInclude(t => t.User)
            .Include(s => s.Gym)
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId && !s.IsDeleted);

        if (session == null)
            throw NotFoundException.Session(sessionId);

        var dto = mapper.Map<SessionDto>(session);
        return ResponseModel<SessionDto>.SuccessResponse(dto);
    }

    public async Task<ResponseModel<SessionQrDto>> GetSessionQrAsync(Guid sessionId)
    {
        var userId = GetCurrentUserId();

        var session = await sessionRepository.Table
            .Include(s => s.Trainer)
                .ThenInclude(t => t.User)
            .Include(s => s.Gym)
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId && !s.IsDeleted);

        if (session == null)
            throw NotFoundException.Session(sessionId);

        if (session.Status != SessionStatus.Paid)
            throw BusinessException.Custom(BusinessErrorCodes.SessionNotPaid, "Yaln?z öd?nilmi? sessiyalar üçün QR kod al?n?r.");

        if (session.IsExpired())
            throw BusinessException.Custom(BusinessErrorCodes.SessionExpired, "Sessiyan?n etibarl?l?q müdd?ti bitib.");

        if (string.IsNullOrEmpty(session.QrToken))
            throw BusinessException.Custom(BusinessErrorCodes.UnknownError, "QR kod tap?lmad?.");

        var dto = new SessionQrDto
        {
            SessionId = session.Id,
            QrToken = session.QrToken,
            TrainerName = session.Trainer.User.FullName,
            GymName = session.Gym?.Name ?? "Online Session",
            GymAddress = session.Gym?.Address ?? "Online",
            ExpiresAt = session.ExpiresAt,
            DaysUntilExpiry = session.ExpiresAt.HasValue
                ? (int)(session.ExpiresAt.Value - DateTime.UtcNow).TotalDays
                : null
        };

        return ResponseModel<SessionQrDto>.SuccessResponse(dto);
    }

    public async Task<ResponseModel<bool>> CancelSessionAsync(Guid sessionId, SessionCancelDto dto)
    {
        var validationResult = await cancelValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            throw BusinessException.ValidationFailed(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

        var userId = GetCurrentUserId();

        var session = await sessionRepository.Table
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId && !s.IsDeleted);

        if (session == null)
            throw NotFoundException.Session(sessionId);

        if (session.Status == SessionStatus.Used)
            throw BusinessException.Custom(BusinessErrorCodes.SessionAlreadyUsed, "?stifad? olunmu? sessiya l??v edil? bilm?z.");

        if (session.Status == SessionStatus.Cancelled || session.Status == SessionStatus.Refunded)
            throw BusinessException.Custom(BusinessErrorCodes.SessionAlreadyCancelled, "Bu sessiya art?q l??v edilib.");

        session.Cancel(dto.Reason);
        sessionRepository.Update(session);
        await sessionRepository.SaveAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }

    // ????????????????????????????????????????????????????????????????
    // QR VERIFY (STAFF/ADMIN)
    // ????????????????????????????????????????????????????????????????

    public async Task<ResponseModel<SessionVerifyResultDto>> VerifyAndUseSessionAsync(SessionVerifyDto dto)
    {
        var validationResult = await verifyValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return ResponseModel<SessionVerifyResultDto>.SuccessResponse(new SessionVerifyResultDto
            {
                IsValid = false,
                Message = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))
            });
        }

        var session = await sessionRepository.Table
            .Include(s => s.User)
            .Include(s => s.Trainer)
                .ThenInclude(t => t.User)
            .Include(s => s.Gym)
            .FirstOrDefaultAsync(s => s.QrToken == dto.QrToken && !s.IsDeleted);

        if (session == null)
        {
            return ResponseModel<SessionVerifyResultDto>.SuccessResponse(new SessionVerifyResultDto
            {
                IsValid = false,
                Message = "Etibars?z QR kod."
            });
        }

        if (session.Status == SessionStatus.Used)
        {
            return ResponseModel<SessionVerifyResultDto>.SuccessResponse(new SessionVerifyResultDto
            {
                IsValid = false,
                Message = "Bu sessiya art?q istifad? olunub.",
                SessionId = session.Id,
                UserName = session.User.FullName,
                UsedAt = session.UsedAt
            });
        }

        if (session.Status != SessionStatus.Paid)
        {
            return ResponseModel<SessionVerifyResultDto>.SuccessResponse(new SessionVerifyResultDto
            {
                IsValid = false,
                Message = $"Sessiya statusu: {session.Status}. Yaln?z öd?nilmi? sessiyalar istifad? oluna bil?r."
            });
        }

        if (session.IsExpired())
        {
            session.MarkAsExpired();
            sessionRepository.Update(session);
            await sessionRepository.SaveAsync();

            return ResponseModel<SessionVerifyResultDto>.SuccessResponse(new SessionVerifyResultDto
            {
                IsValid = false,
                Message = "Sessiyan?n etibarl?l?q müdd?ti bitib.",
                SessionId = session.Id,
                UserName = session.User.FullName
            });
        }

        session.Use();

        var trainer = session.Trainer;
        trainer.IncrementSessions();

        var previousSessions = await sessionRepository.Table
            .Where(s => s.TrainerId == trainer.Id && s.UserId == session.UserId && s.Status == SessionStatus.Used && s.Id != session.Id)
            .AnyAsync();

        if (!previousSessions)
            trainer.IncrementClients();

        sessionRepository.Update(session);
        trainerRepository.Update(trainer);
        await sessionRepository.SaveAsync();

        return ResponseModel<SessionVerifyResultDto>.SuccessResponse(new SessionVerifyResultDto
        {
            IsValid = true,
            Message = "Giri? u?urla t?sdiql?ndi!",
            SessionId = session.Id,
            UserName = session.User.FullName,
            TrainerName = session.Trainer.User.FullName,
            GymName = session.Gym?.Name ?? "Online",
            UsedAt = session.UsedAt
        });
    }

    // ????????????????????????????????????????????????????????????????
    // TRAINER ENDPOINTS
    // ????????????????????????????????????????????????????????????????

    public async Task<ResponseModel<List<SessionDto>>> GetTrainerSessionsAsync()
    {
        var trainer = await GetCurrentTrainerAsync();

        var sessions = await sessionRepository.Table
            .Include(s => s.User)
            .Include(s => s.Gym)
            .Include(s => s.Trainer)
                .ThenInclude(t => t.User)
            .Where(s => s.TrainerId == trainer.Id && !s.IsDeleted)
            .OrderByDescending(s => s.CreatedAt)
            .AsNoTracking()
            .ToListAsync();

        var dtos = mapper.Map<List<SessionDto>>(sessions);
        return ResponseModel<List<SessionDto>>.SuccessResponse(dtos);
    }

    // ????????????????????????????????????????????????????????????????
    // GYM OWNER ENDPOINTS
    // ????????????????????????????????????????????????????????????????

    public async Task<ResponseModel<List<SessionDto>>> GetGymSessionsAsync()
    {
        var gymId = await GetCurrentUserGymIdAsync();
        if (gymId == null)
            throw NotFoundException.GymForOwner();

        var sessions = await sessionRepository.Table
            .Include(s => s.User)
            .Include(s => s.Trainer)
                .ThenInclude(t => t.User)
            .Include(s => s.Gym)
            .Where(s => s.GymId == gymId && !s.IsDeleted)
            .OrderByDescending(s => s.CreatedAt)
            .AsNoTracking()
            .ToListAsync();

        var dtos = mapper.Map<List<SessionDto>>(sessions);
        return ResponseModel<List<SessionDto>>.SuccessResponse(dtos);
    }

    // ????????????????????????????????????????????????????????????????
    // PAYMENT CALLBACK
    // ????????????????????????????????????????????????????????????????

    public async Task<ResponseModel<bool>> ConfirmPaymentAsync(Guid sessionId, Guid paymentId)
    {
        var session = await sessionRepository.GetByIdAsync(sessionId);
        if (session == null)
            throw NotFoundException.Session(sessionId);

        if (session.Status != SessionStatus.Pending)
            throw BusinessException.Custom(BusinessErrorCodes.SessionAlreadyProcessed, "Bu sessiya art?q öd?nilib v? ya l??v edilib.");

        session.MarkAsPaid(paymentId);
        sessionRepository.Update(session);
        await sessionRepository.SaveAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }
}
