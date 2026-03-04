using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Startup_Project.Application.Abstractions.UnitOfWork;
using Startup_Project.Application.Dtos.Payment;
using Startup_Project.Application.Interfaces;
using Startup_Project.Application.Models;
using Startup_Project.Application.Services.Interfaces;
using Startup_Project.Domain.Entities;
using Startup_Project.Domain.Enums;
using Startup_Project.Domain.Enums.Errors;
using Startup_Project.Domain.Exceptions;
using System.Security.Claims;

namespace Startup_Project.Application.Services.Concretes;

public class SessionPaymentService(
    IApplicationDbContext context,
    IBankService bankService,
    IUnitOfWork unitOfWork,
    IHttpContextAccessor httpContextAccessor,
    ILogger<SessionPaymentService> logger) : ISessionPaymentService
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

    #endregion

    #region ==================== CREATE PAYMENT ====================

    public async Task<ResponseModel<SessionPaymentResponseDto>> CreatePaymentAsync(SessionPaymentRequestDto request)
    {
        var studentId = GetCurrentUserId();

        var session = await context.ScheduledSessions
            .Include(s => s.Trainer)
                .ThenInclude(t => t.User)
            .FirstOrDefaultAsync(s => s.Id == request.ScheduledSessionId
                && !s.IsDeleted
                && s.Status != ScheduledSessionStatus.Cancelled);

        if (session is null)
            throw BusinessException.Custom(BusinessErrorCodes.ScheduledSessionNotFound, "Seans tapilmadi.");

        if (session.Status == ScheduledSessionStatus.Completed)
            throw BusinessException.Custom(BusinessErrorCodes.ScheduledSessionInvalidStatus, "Tamamlanmis seansa qosulmaq olmaz.");

        if (session.ScheduledTime <= DateTime.UtcNow)
            throw BusinessException.Custom(BusinessErrorCodes.SessionExpired, "Kecmis vaxta aid seansa odenis etmek olmaz.");

        // Artiq bu session ucun ugurlu odenis varmisa yoxla
        var alreadyPaid = await context.SessionPayments
            .AnyAsync(p => p.StudentId == studentId
                && p.ScheduledSessionId == session.Id
                && p.Status == PaymentStatus.Successful);

        if (alreadyPaid)
            throw BusinessException.Custom(BusinessErrorCodes.PaymentAlreadyProcessed, "Bu seans ucun artiq odenis edilib.");

        // Pending odenis varsa onu sil (yeniden cehd)
        var pendingPayment = await context.SessionPayments
            .FirstOrDefaultAsync(p => p.StudentId == studentId
                && p.ScheduledSessionId == session.Id
                && p.Status == PaymentStatus.Pending);

        if (pendingPayment is not null)
        {
            pendingPayment.MarkAsFailed("Yeni odenis cehdi - kohne legv edildi.");
        }

        var amount = session.PricePerPerson;
        var sessionTitle = session.Title ?? session.ExerciseType;

        // Yeni payment yarat
        var payment = new SessionPayment(
            studentId: studentId,
            trainerId: session.TrainerId,
            scheduledSessionId: session.Id,
            amount: amount);

        // Banka sorgu at
        var bankResponse = await bankService.CreateSessionAsync(amount, $"FitZone Session: {sessionTitle}");

        payment.SetBankSessionId(bankResponse.SessionId);

        context.SessionPayments.Add(payment);
        await context.SaveChangesAsync();

        return ResponseModel<SessionPaymentResponseDto>.SuccessResponse(new SessionPaymentResponseDto
        {
            OrderId = payment.OrderId,
            RedirectUrl = bankResponse.RedirectUrl,
            BankSessionId = bankResponse.SessionId
        });
    }

    #endregion

    #region ==================== HANDLE CALLBACK ====================

    public async Task<ResponseModel<bool>> HandleCallbackAsync(string sessionId)
    {
        var payment = await context.SessionPayments
            .Include(p => p.ScheduledSession)
                .ThenInclude(s => s.Trainer)
            .FirstOrDefaultAsync(p => p.BankSessionId == sessionId);

        if (payment is null)
            throw NotFoundException.Payment(sessionId);

        if (payment.IsSuccessful)
            return ResponseModel<bool>.SuccessResponse(true);

        // Bankdan status yoxla
        var isApproved = await bankService.CheckPaymentStatusAsync(sessionId);

        if (!isApproved)
        {
            payment.MarkAsFailed("Bank terefinden tesdiqlenmeyi.");
            await unitOfWork.SaveChangesAsync();
            throw BusinessException.Custom(BusinessErrorCodes.PaymentFailed, "Odenis bank terefinden tesdiqlenmeyi.");
        }

        // 1. Odenisi ugurlu isare et
        payment.MarkAsSuccess();

        // 2. Session-u tesdiq et (eger pending-dirse)
        var session = payment.ScheduledSession;
        if (session.Status == ScheduledSessionStatus.Pending)
            session.Confirm();

        // 3. Trainer-Student bagliligi
        var alreadyLinked = await context.TrainerStudents
            .AnyAsync(ts => ts.TrainerId == payment.TrainerId
                && ts.StudentId == payment.StudentId
                && !ts.IsDeleted);

        if (!alreadyLinked)
        {
            var trainerStudent = new TrainerStudent(
                trainerId: payment.TrainerId,
                studentId: payment.StudentId,
                program: session.Title ?? session.ExerciseType,
                totalSessions: 1);

            context.TrainerStudents.Add(trainerStudent);

            var trainer = session.Trainer;
            trainer.IncrementClients();
        }
        else
        {
            var existing = await context.TrainerStudents
                .FirstAsync(ts => ts.TrainerId == payment.TrainerId
                    && ts.StudentId == payment.StudentId
                    && !ts.IsDeleted);

            existing.UpdateTotalSessions(existing.TotalSessions + 1);
        }

        // 4. Trainer session sayini artir
        var trainerEntity = session.Trainer;
        trainerEntity.IncrementSessions();

        // Tek SaveChanges — butun deyisiklikleri birdefeye yaz
        await context.SaveChangesAsync();

        logger.LogInformation("Session payment callback: SUCCESS — SessionId={SessionId}", sessionId);

        return ResponseModel<bool>.SuccessResponse(true);
    }

    #endregion

    #region ==================== GET PAYMENT STATUS ====================

    public async Task<ResponseModel<SessionPaymentStatusDto>> GetPaymentStatusAsync(string orderId)
    {
        var userId = GetCurrentUserId();

        var payment = await context.SessionPayments
            .Include(p => p.Trainer).ThenInclude(t => t.User)
            .Include(p => p.ScheduledSession)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.OrderId == orderId && p.StudentId == userId);

        if (payment is null)
            throw NotFoundException.Payment(orderId);

        return ResponseModel<SessionPaymentStatusDto>.SuccessResponse(MapToDto(payment));
    }

    #endregion

    #region ==================== GET MY PAYMENTS ====================

    public async Task<ResponseModel<MyPaymentsResponseDto>> GetMyPaymentsAsync()
    {
        var userId = GetCurrentUserId();

        var payments = await context.SessionPayments
            .Include(p => p.Trainer).ThenInclude(t => t.User)
            .Include(p => p.ScheduledSession)
            .Where(p => p.StudentId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .AsNoTracking()
            .ToListAsync();

        var totalSpent = payments
            .Where(p => p.Status == PaymentStatus.Successful)
            .Sum(p => p.Amount);

        return ResponseModel<MyPaymentsResponseDto>.SuccessResponse(new MyPaymentsResponseDto
        {
            TotalCount = payments.Count,
            TotalSpent = totalSpent,
            Payments = payments.Select(MapToDto).ToList()
        });
    }

    #endregion

    #region ==================== MAPPER ====================

    private static SessionPaymentStatusDto MapToDto(SessionPayment p) => new()
    {
        Id = p.Id,
        OrderId = p.OrderId,
        Amount = p.Amount,
        Status = p.Status.ToString().ToLowerInvariant(),
        TrainerName = p.Trainer?.User?.FullName ?? "Unknown",
        SessionTitle = p.ScheduledSession?.Title ?? p.ScheduledSession?.ExerciseType ?? "Session",
        SessionDate = p.ScheduledSession?.ScheduledTime.Date ?? DateTime.MinValue,
        SessionTime = p.ScheduledSession?.ScheduledTime.ToString("HH:mm") ?? "--:--",
        CreatedAt = p.CreatedAt
    };

    #endregion
}
