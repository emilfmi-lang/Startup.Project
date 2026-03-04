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

public class PaymentService(
    IApplicationDbContext context,
    IBankService bankService,
    IUnitOfWork unitOfWork,
    IHttpContextAccessor httpContextAccessor,
    ILogger<PaymentService> logger) : IPaymentService
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

        if (payment.IsSuccessful)
            return ResponseModel<bool>.SuccessResponse(true);

        var isApproved = await bankService.CheckPaymentStatusAsync(sessionId);

        if (!isApproved)
        {
            payment.MarkAsFailed("Bank terefinden tesdiqlenmeyi.");
            await unitOfWork.SaveChangesAsync();
            throw BusinessException.Custom(BusinessErrorCodes.PaymentFailed, "Odenis bank terefinden tesdiqlenmeyi.");
        }

        var package = payment.Package;

        if (package is null)
        {
            logger.LogError("Payment callback: Package is NULL for SessionId={SessionId}, PackageId={PackageId}",
                sessionId, payment.PackageId);
            payment.MarkAsFailed("Paket tapilmadi.");
            await unitOfWork.SaveChangesAsync();
            throw BusinessException.Custom(BusinessErrorCodes.PackageNotFound, "Paket tapilmadi.");
        }

        logger.LogInformation("Payment callback: Processing — SessionId={SessionId}, Package={Name}, Duration={Days}, UserId={UserId}",
            sessionId, package.Name, package.DurationInDays, payment.UserId);

        // 1. Payment -> Successful
        payment.MarkAsSuccess();

        // 2. Subscription yarat/uzat
        var activeSubscription = await context.Subscriptions
            .Where(s => s.UserId == payment.UserId
                && s.Status == SubscriptionStatus.Active
                && s.EndDate > DateTime.UtcNow
                && !s.IsDeleted)
            .OrderByDescending(s => s.EndDate)
            .FirstOrDefaultAsync();

        if (activeSubscription is not null)
        {
            activeSubscription.Extend(package.DurationInDays);
            logger.LogInformation("Payment callback: Extended existing subscription, NewEndDate={EndDate}", activeSubscription.EndDate);
        }
        else
        {
            var subscription = new Subscription(payment.UserId, package, DateTime.UtcNow);
            context.Subscriptions.Add(subscription);
            logger.LogInformation("Payment callback: New subscription created — StartDate={Start}, EndDate={End}",
                subscription.StartDate, subscription.EndDate);
        }

        // Tek SaveChanges — Payment UPDATE + Subscription INSERT birdəfəyə
        await context.SaveChangesAsync();

        logger.LogInformation("Payment callback: SUCCESS — SessionId={SessionId}, Subscription saved to DB", sessionId);

        return ResponseModel<bool>.SuccessResponse(true);
    }

    #endregion
}
