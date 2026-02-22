using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
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

public class PaymentService(
    IApplicationDbContext context,
    IBankService bankService,
    IUnitOfWork unitOfWork,
    IHttpContextAccessor httpContextAccessor) : IPaymentService
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

    #region ==================== CREATE PACKAGE PAYMENT ====================

    public async Task<ResponseModel<PaymentResponseDto>> CreatePackagePaymentAsync(PaymentRequestDto request)
    {
        var userId = GetCurrentUserId();

        var package = await context.Packages
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.PackageId && !p.IsDeleted);

        if (package is null)
            throw NotFoundException.Package(request.PackageId);

        // Eyni paket ucun pending odenis varsa legv et (yeniden cehd)
        var pendingPayment = await context.Payments
            .FirstOrDefaultAsync(p => p.UserId == userId
                && p.PackageId == request.PackageId
                && p.Status == PaymentStatus.Pending);

        if (pendingPayment is not null)
            pendingPayment.MarkAsFailed("Yeni odenis cehdi - kohne legv edildi.");

        // Yeni payment yarat
        var payment = new Payment(
            userId: userId,
            packageId: package.Id,
            amount: package.Price);

        // Banka sorgu at
        var bankResponse = await bankService.CreateSessionAsync(package.Price, package.Name);

        payment.SetBankSessionId(bankResponse.SessionId);

        context.Payments.Add(payment);
        await context.SaveChangesAsync();

        return ResponseModel<PaymentResponseDto>.SuccessResponse(bankResponse);
    }

    #endregion

    #region ==================== HANDLE PACKAGE CALLBACK ====================

    public async Task<ResponseModel<bool>> HandlePackageCallbackAsync(string sessionId)
    {
        var payment = await context.Payments
            .Include(p => p.Package)
            .FirstOrDefaultAsync(p => p.SessionId == sessionId);

        if (payment is null)
            throw NotFoundException.Payment(sessionId);

        // Idempotent: artiq ugurlu olubsa tekrar islemir
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

        // ===== ATOMIC TRANSACTION =====
        // 1) Payment -> Successful
        // 2) Subscription -> yaradilir ve ya uzadilir (extension)
        // Xeta olsa butun deyisiklikler geri qaytarilir.

        await unitOfWork.BeginTransactionAsync();

        try
        {
            // 1. Odenisi ugurlu isare et
            payment.MarkAsSuccess();

            // 2. Subscription mentiqi
            var package = payment.Package;

            // Eyni gym paketin aktiv abuneliyini yoxla
            var activeSubscription = await context.Subscriptions
                .Where(s => s.UserId == payment.UserId
                    && s.Status == SubscriptionStatus.Active
                    && s.EndDate > DateTime.UtcNow
                    && !s.IsDeleted)
                .OrderByDescending(s => s.EndDate)
                .FirstOrDefaultAsync();

            if (activeSubscription is not null)
            {
                // EXTENSION: movcud abunelyin EndDate-inden baslayir
                activeSubscription.Extend(package.DurationInDays);
            }
            else
            {
                // YENI: bugun baslayir
                var subscription = new Subscription(payment.UserId, package, DateTime.UtcNow);
                context.Subscriptions.Add(subscription);
            }

            await unitOfWork.CommitAsync();
        }
        catch
        {
            await unitOfWork.RollbackAsync();
            throw;
        }

        return ResponseModel<bool>.SuccessResponse(true);
    }

    #endregion
}
