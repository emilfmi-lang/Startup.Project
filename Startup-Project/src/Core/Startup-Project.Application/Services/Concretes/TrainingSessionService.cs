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
using System.Security.Claims;

namespace Startup_Project.Application.Services.Concretes;

public class TrainingSessionService : ITrainingSessionService
{
    private readonly IGenericRepository<TrainingSession> _sessionRepository;
    private readonly IGenericRepository<Trainer> _trainerRepository;
    private readonly IGenericRepository<Gym> _gymRepository;
    private readonly IGenericRepository<Payment> _paymentRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;
    private readonly IValidator<SessionPurchaseDto> _purchaseValidator;
    private readonly IValidator<SessionCancelDto> _cancelValidator;
    private readonly IValidator<SessionVerifyDto> _verifyValidator;

    public TrainingSessionService(
        IGenericRepository<TrainingSession> sessionRepository,
        IGenericRepository<Trainer> trainerRepository,
        IGenericRepository<Gym> gymRepository,
        IGenericRepository<Payment> paymentRepository,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper,
        IValidator<SessionPurchaseDto> purchaseValidator,
        IValidator<SessionCancelDto> cancelValidator,
        IValidator<SessionVerifyDto> verifyValidator)
    {
        _sessionRepository = sessionRepository;
        _trainerRepository = trainerRepository;
        _gymRepository = gymRepository;
        _paymentRepository = paymentRepository;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
        _purchaseValidator = purchaseValidator;
        _cancelValidator = cancelValidator;
        _verifyValidator = verifyValidator;
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return null;
        return userId;
    }

    private async Task<Guid?> GetCurrentUserGymIdAsync()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return null;

        var gym = await _gymRepository.Table
            .FirstOrDefaultAsync(g => g.OwnerId == userId && !g.IsDeleted);
        return gym?.Id;
    }

    private async Task<Trainer?> GetCurrentTrainerAsync()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return null;

        return await _trainerRepository.Table
            .FirstOrDefaultAsync(t => t.UserId == userId && !t.IsDeleted);
    }

    // ??????????????????????????????????????????????????????????????
    // USER ENDPOINTS
    // ??????????????????????????????????????????????????????????????

    public async Task<ResponseModel<string>> PurchaseSessionAsync(SessionPurchaseDto dto)
    {
        // Validation
        var validationResult = await _purchaseValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            return ResponseModel<string>.FailureResponse(errors);
        }

        var userId = GetCurrentUserId();
        if (userId == null)
            return ResponseModel<string>.FailureResponse("Giris etmelisiniz.");

        var trainer = await _trainerRepository.Table
            .Include(t => t.Gym)
            .FirstOrDefaultAsync(t => t.Id == dto.TrainerId && !t.IsDeleted);

        if (trainer == null)
            return ResponseModel<string>.FailureResponse("Mesqci tapilmadi.");

        if (trainer.Status != TrainerStatus.Active)
            return ResponseModel<string>.FailureResponse("Bu mesqci hazirda aktiv deyil.");

        // Online trainer verify olunmalidi
        if (trainer.IsOnlineTrainer && !trainer.IsVerified)
            return ResponseModel<string>.FailureResponse("Bu online mesqci henuz tesdiqlenib.");

        // Yeni sessiya yarat
        var session = new TrainingSession(
            trainerId: trainer.Id,
            userId: userId.Value,
            gymId: trainer.GymId ?? Guid.Empty,
            price: trainer.SessionPrice
        );

        await _sessionRepository.AddAsync(session);
        await _sessionRepository.SaveAsync();

        return ResponseModel<string>.SuccessResponse(session.Id.ToString());
    }

    public async Task<ResponseModel<List<SessionDto>>> GetMySessionsAsync()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return ResponseModel<List<SessionDto>>.FailureResponse("Giris etmelisiniz.");

        var sessions = await _sessionRepository.Table
            .Include(s => s.Trainer)
                .ThenInclude(t => t.User)
            .Include(s => s.Gym)
            .Where(s => s.UserId == userId && !s.IsDeleted)
            .OrderByDescending(s => s.CreatedAt)
            .AsNoTracking()
            .ToListAsync();

        var dtos = _mapper.Map<List<SessionDto>>(sessions);
        return ResponseModel<List<SessionDto>>.SuccessResponse(dtos);
    }

    public async Task<ResponseModel<SessionDto>> GetSessionByIdAsync(Guid sessionId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return ResponseModel<SessionDto>.FailureResponse("Giris etmelisiniz.");

        var session = await _sessionRepository.Table
            .Include(s => s.Trainer)
                .ThenInclude(t => t.User)
            .Include(s => s.Gym)
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId && !s.IsDeleted);

        if (session == null)
            return ResponseModel<SessionDto>.FailureResponse("Sessiya tapilmadi.");

        var dto = _mapper.Map<SessionDto>(session);
        return ResponseModel<SessionDto>.SuccessResponse(dto);
    }

    public async Task<ResponseModel<SessionQrDto>> GetSessionQrAsync(Guid sessionId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return ResponseModel<SessionQrDto>.FailureResponse("Giris etmelisiniz.");

        var session = await _sessionRepository.Table
            .Include(s => s.Trainer)
                .ThenInclude(t => t.User)
            .Include(s => s.Gym)
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId && !s.IsDeleted);

        if (session == null)
            return ResponseModel<SessionQrDto>.FailureResponse("Sessiya tapilmadi.");

        if (session.Status != SessionStatus.Paid)
            return ResponseModel<SessionQrDto>.FailureResponse("Yalniz odenilmis sessiyalar ucun QR kod alinir.");

        if (session.IsExpired())
            return ResponseModel<SessionQrDto>.FailureResponse("Sessiyanin etibarliq muddeti bitib.");

        if (string.IsNullOrEmpty(session.QrToken))
            return ResponseModel<SessionQrDto>.FailureResponse("QR kod tapilmadi.");

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
        // Validation
        var validationResult = await _cancelValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            return ResponseModel<bool>.FailureResponse(errors);
        }

        var userId = GetCurrentUserId();
        if (userId == null)
            return ResponseModel<bool>.FailureResponse("Giris etmelisiniz.");

        var session = await _sessionRepository.Table
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId && !s.IsDeleted);

        if (session == null)
            return ResponseModel<bool>.FailureResponse("Sessiya tapilmadi.");

        if (session.Status == SessionStatus.Used)
            return ResponseModel<bool>.FailureResponse("Istifade olunmus sessiya legv edile bilmez.");

        if (session.Status == SessionStatus.Cancelled || session.Status == SessionStatus.Refunded)
            return ResponseModel<bool>.FailureResponse("Bu sessiya artiq legv edilib.");

        session.Cancel(dto.Reason);
        _sessionRepository.Update(session);
        await _sessionRepository.SaveAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }

    // ??????????????????????????????????????????????????????????????
    // QR VERIFY (STAFF/ADMIN)
    // ??????????????????????????????????????????????????????????????

    public async Task<ResponseModel<SessionVerifyResultDto>> VerifyAndUseSessionAsync(SessionVerifyDto dto)
    {
        // Validation
        var validationResult = await _verifyValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            return ResponseModel<SessionVerifyResultDto>.SuccessResponse(new SessionVerifyResultDto
            {
                IsValid = false,
                Message = errors
            });
        }

        var session = await _sessionRepository.Table
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
                Message = "Etibarsiz QR kod."
            });
        }

        if (session.Status == SessionStatus.Used)
        {
            return ResponseModel<SessionVerifyResultDto>.SuccessResponse(new SessionVerifyResultDto
            {
                IsValid = false,
                Message = "Bu sessiya artiq istifade olunub.",
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
                Message = $"Sessiya statusu: {session.Status}. Yalniz odenilmis sessiyalar istifade oluna biler."
            });
        }

        if (session.IsExpired())
        {
            session.MarkAsExpired();
            _sessionRepository.Update(session);
            await _sessionRepository.SaveAsync();

            return ResponseModel<SessionVerifyResultDto>.SuccessResponse(new SessionVerifyResultDto
            {
                IsValid = false,
                Message = "Sessiyanin etibarliq muddeti bitib.",
                SessionId = session.Id,
                UserName = session.User.FullName
            });
        }

        // Sessiyani istifade et
        session.Use();

        // Trainer statistikasini yenile
        var trainer = session.Trainer;
        trainer.IncrementSessions();

        // Ilk defe bu user ile isleyirse, client say artirilir
        var previousSessions = await _sessionRepository.Table
            .Where(s => s.TrainerId == trainer.Id && s.UserId == session.UserId && s.Status == SessionStatus.Used && s.Id != session.Id)
            .AnyAsync();

        if (!previousSessions)
        {
            trainer.IncrementClients();
        }

        _sessionRepository.Update(session);
        _trainerRepository.Update(trainer);
        await _sessionRepository.SaveAsync();

        return ResponseModel<SessionVerifyResultDto>.SuccessResponse(new SessionVerifyResultDto
        {
            IsValid = true,
            Message = "Giris ugurla tesdislendi!",
            SessionId = session.Id,
            UserName = session.User.FullName,
            TrainerName = session.Trainer.User.FullName,
            GymName = session.Gym?.Name ?? "Online",
            UsedAt = session.UsedAt
        });
    }

    // ??????????????????????????????????????????????????????????????
    // TRAINER ENDPOINTS
    // ??????????????????????????????????????????????????????????????

    public async Task<ResponseModel<List<SessionDto>>> GetTrainerSessionsAsync()
    {
        var trainer = await GetCurrentTrainerAsync();
        if (trainer == null)
            return ResponseModel<List<SessionDto>>.FailureResponse("Mesqci profiliniz tapilmadi.");

        var sessions = await _sessionRepository.Table
            .Include(s => s.User)
            .Include(s => s.Gym)
            .Include(s => s.Trainer)
                .ThenInclude(t => t.User)
            .Where(s => s.TrainerId == trainer.Id && !s.IsDeleted)
            .OrderByDescending(s => s.CreatedAt)
            .AsNoTracking()
            .ToListAsync();

        var dtos = _mapper.Map<List<SessionDto>>(sessions);
        return ResponseModel<List<SessionDto>>.SuccessResponse(dtos);
    }

    // ??????????????????????????????????????????????????????????????
    // GYM OWNER ENDPOINTS
    // ??????????????????????????????????????????????????????????????

    public async Task<ResponseModel<List<SessionDto>>> GetGymSessionsAsync()
    {
        var gymId = await GetCurrentUserGymIdAsync();
        if (gymId == null)
            return ResponseModel<List<SessionDto>>.FailureResponse("Zaliniz tapilmadi.");

        var sessions = await _sessionRepository.Table
            .Include(s => s.User)
            .Include(s => s.Trainer)
                .ThenInclude(t => t.User)
            .Include(s => s.Gym)
            .Where(s => s.GymId == gymId && !s.IsDeleted)
            .OrderByDescending(s => s.CreatedAt)
            .AsNoTracking()
            .ToListAsync();

        var dtos = _mapper.Map<List<SessionDto>>(sessions);
        return ResponseModel<List<SessionDto>>.SuccessResponse(dtos);
    }

    // ??????????????????????????????????????????????????????????????
    // PAYMENT CALLBACK
    // ??????????????????????????????????????????????????????????????

    public async Task<ResponseModel<bool>> ConfirmPaymentAsync(Guid sessionId, Guid paymentId)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId);
        if (session == null)
            return ResponseModel<bool>.FailureResponse("Sessiya tapilmadi.");

        if (session.Status != SessionStatus.Pending)
            return ResponseModel<bool>.FailureResponse("Bu sessiya artiq odenilib ve ya legv edilib.");

        session.MarkAsPaid(paymentId);
        _sessionRepository.Update(session);
        await _sessionRepository.SaveAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }
}
