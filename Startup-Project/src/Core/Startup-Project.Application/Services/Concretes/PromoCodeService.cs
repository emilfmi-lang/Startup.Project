using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Startup_Project.Application.Dtos.PromoCode;
using Startup_Project.Application.Interfaces;
using Startup_Project.Application.Models;
using Startup_Project.Application.Services.Interfaces;
using Startup_Project.Domain.Abstractions.Repositories;
using Startup_Project.Domain.Entities;
using Startup_Project.Domain.Enums;
using Startup_Project.Domain.Enums.Errors;
using Startup_Project.Domain.Exceptions;
using System.Security.Claims;

namespace Startup_Project.Application.Services.Concretes;

public class PromoCodeService(
    IGenericRepository<PromoCode> promoCodeRepository,
    IGenericRepository<PromoCodeUsage> usageRepository,
    IGenericRepository<Package> packageRepository,
    IApplicationDbContext context,
    IHttpContextAccessor httpContextAccessor,
    IValidator<ApplyPromoCodeDto> applyValidator) : IPromoCodeService
{
    private Guid GetCurrentUserId()
    {
        var userId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            throw BusinessException.UnauthorizedAccess();
        return userGuid;
    }

    #region ==================== PACKAGE PROMO ====================

    public async Task<ResponseModel<PromoCodeResultDto>> ValidateAsync(string code, Guid packageId)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();

        var promoCode = await promoCodeRepository.Table
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Code == normalizedCode && !p.IsDeleted);

        if (promoCode == null)
            throw NotFoundException.PromoCode(code);

        if (!promoCode.IsForTarget(PromoCodeTargetType.Package))
            throw BusinessException.Custom(BusinessErrorCodes.PromoCodeTargetMismatch,
                $"'{code}' promokodu paketler ucun kecerli deyil.");

        var package = await packageRepository.GetByIdAsync(packageId);
        if (package == null)
            throw NotFoundException.Package(packageId);

        ValidatePromoCode(promoCode, packageId, package.Price);

        var discountAmount = promoCode.CalculateDiscount(package.Price);
        var finalPrice = Math.Max(0, package.Price - discountAmount);

        return ResponseModel<PromoCodeResultDto>.SuccessResponse(new PromoCodeResultDto
        {
            IsValid = true,
            OriginalPrice = package.Price,
            DiscountAmount = discountAmount,
            FinalPrice = finalPrice,
            DiscountType = promoCode.DiscountType.ToString(),
            DiscountValue = promoCode.DiscountValue,
            Message = $"Promokod kecerlidir. {discountAmount:F2} AZN endirim tetbiq olunacaq."
        });
    }

    public async Task<ResponseModel<PromoCodeResultDto>> ApplyAsync(ApplyPromoCodeDto dto)
    {
        var validationResult = await applyValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            throw BusinessException.ValidationFailed(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

        var userId = GetCurrentUserId();
        var normalizedCode = dto.Code.Trim().ToUpperInvariant();

        var promoCode = await promoCodeRepository.Table
            .FirstOrDefaultAsync(p => p.Code == normalizedCode && !p.IsDeleted);

        if (promoCode == null)
            throw NotFoundException.PromoCode(dto.Code);

        if (!promoCode.IsForTarget(PromoCodeTargetType.Package))
            throw BusinessException.Custom(BusinessErrorCodes.PromoCodeTargetMismatch,
                $"'{dto.Code}' promokodu paketler ucun kecerli deyil.");

        var package = await packageRepository.GetByIdAsync(dto.PackageId);
        if (package == null)
            throw NotFoundException.Package(dto.PackageId);

        var alreadyUsed = await usageRepository.AnyAsync(
            u => u.PromoCodeId == promoCode.Id && u.UserId == userId);

        if (alreadyUsed)
            throw BusinessException.PromoCodeAlreadyUsed(dto.Code);

        ValidatePromoCode(promoCode, dto.PackageId, package.Price);

        var discountAmount = promoCode.CalculateDiscount(package.Price);
        var finalPrice = Math.Max(0, package.Price - discountAmount);

        try
        {
            promoCode.IncrementUsedCount();
            promoCodeRepository.Update(promoCode);

            var usage = new PromoCodeUsage(promoCode.Id, userId, discountAmount, packageId: dto.PackageId);
            await usageRepository.AddAsync(usage);

            await promoCodeRepository.SaveAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw ConflictException.PromoCodeConcurrency(dto.Code);
        }

        return ResponseModel<PromoCodeResultDto>.SuccessResponse(new PromoCodeResultDto
        {
            IsValid = true,
            OriginalPrice = package.Price,
            DiscountAmount = discountAmount,
            FinalPrice = finalPrice,
            DiscountType = promoCode.DiscountType.ToString(),
            DiscountValue = promoCode.DiscountValue,
            Message = $"Promokod ugurla tetbiq edildi. {discountAmount:F2} AZN endirim."
        });
    }

    #endregion

    #region ==================== SESSION PROMO ====================

    public async Task<ResponseModel<PromoCodeResultDto>> ValidateForSessionAsync(string code, Guid scheduledSessionId)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();

        var promoCode = await promoCodeRepository.Table
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Code == normalizedCode && !p.IsDeleted);

        if (promoCode == null)
            throw NotFoundException.PromoCode(code);

        if (!promoCode.IsForTarget(PromoCodeTargetType.Session))
            throw BusinessException.Custom(BusinessErrorCodes.PromoCodeTargetMismatch,
                $"'{code}' promokodu sessiyalar ucun kecerli deyil.");

        var session = await context.ScheduledSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == scheduledSessionId && !s.IsDeleted);

        if (session == null)
            throw BusinessException.Custom(BusinessErrorCodes.ScheduledSessionNotFound, "Seans tapilmadi.");

        if (promoCode.TrainerId.HasValue && promoCode.TrainerId != session.TrainerId)
            throw BusinessException.Custom(BusinessErrorCodes.PromoCodeTrainerMismatch,
                $"'{code}' promokodu bu trainere aid deyil.");

        ValidateSessionPromoCode(promoCode, session.PricePerPerson);

        var discountAmount = promoCode.CalculateDiscount(session.PricePerPerson);
        var finalPrice = Math.Max(0, session.PricePerPerson - discountAmount);

        return ResponseModel<PromoCodeResultDto>.SuccessResponse(new PromoCodeResultDto
        {
            IsValid = true,
            OriginalPrice = session.PricePerPerson,
            DiscountAmount = discountAmount,
            FinalPrice = finalPrice,
            DiscountType = promoCode.DiscountType.ToString(),
            DiscountValue = promoCode.DiscountValue,
            Message = $"Promokod kecerlidir. {discountAmount:F2} AZN endirim tetbiq olunacaq."
        });
    }

    public async Task<ResponseModel<PromoCodeResultDto>> ApplyForSessionAsync(ApplySessionPromoCodeDto dto)
    {
        var userId = GetCurrentUserId();
        var normalizedCode = dto.Code.Trim().ToUpperInvariant();

        var promoCode = await promoCodeRepository.Table
            .FirstOrDefaultAsync(p => p.Code == normalizedCode && !p.IsDeleted);

        if (promoCode == null)
            throw NotFoundException.PromoCode(dto.Code);

        if (!promoCode.IsForTarget(PromoCodeTargetType.Session))
            throw BusinessException.Custom(BusinessErrorCodes.PromoCodeTargetMismatch,
                $"'{dto.Code}' promokodu sessiyalar ucun kecerli deyil.");

        var session = await context.ScheduledSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == dto.ScheduledSessionId && !s.IsDeleted);

        if (session == null)
            throw BusinessException.Custom(BusinessErrorCodes.ScheduledSessionNotFound, "Seans tapilmadi.");

        if (promoCode.TrainerId.HasValue && promoCode.TrainerId != session.TrainerId)
            throw BusinessException.Custom(BusinessErrorCodes.PromoCodeTrainerMismatch,
                $"'{dto.Code}' promokodu bu trainere aid deyil.");

        var alreadyUsed = await usageRepository.AnyAsync(
            u => u.PromoCodeId == promoCode.Id && u.UserId == userId);

        if (alreadyUsed)
            throw BusinessException.PromoCodeAlreadyUsed(dto.Code);

        ValidateSessionPromoCode(promoCode, session.PricePerPerson);

        var discountAmount = promoCode.CalculateDiscount(session.PricePerPerson);
        var finalPrice = Math.Max(0, session.PricePerPerson - discountAmount);

        try
        {
            promoCode.IncrementUsedCount();
            promoCodeRepository.Update(promoCode);

            var usage = new PromoCodeUsage(promoCode.Id, userId, discountAmount, scheduledSessionId: dto.ScheduledSessionId);
            await usageRepository.AddAsync(usage);

            await promoCodeRepository.SaveAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw ConflictException.PromoCodeConcurrency(dto.Code);
        }

        return ResponseModel<PromoCodeResultDto>.SuccessResponse(new PromoCodeResultDto
        {
            IsValid = true,
            OriginalPrice = session.PricePerPerson,
            DiscountAmount = discountAmount,
            FinalPrice = finalPrice,
            DiscountType = promoCode.DiscountType.ToString(),
            DiscountValue = promoCode.DiscountValue,
            Message = $"Promokod ugurla tetbiq edildi. {discountAmount:F2} AZN endirim."
        });
    }

    #endregion

    #region ==================== PRIVATE VALIDATORS ====================

    private static void ValidatePromoCode(PromoCode promoCode, Guid packageId, decimal orderAmount)
    {
        if (!promoCode.IsActive)
            throw BusinessException.PromoCodeDeactivated(promoCode.Code);

        if (!promoCode.IsValidForDate())
            throw BusinessException.PromoCodeExpired(promoCode.Code, promoCode.EndDate);

        if (!promoCode.HasAvailableUsage())
            throw BusinessException.PromoCodeUsageLimitExceeded(promoCode.Code, promoCode.UsageLimit);

        if (!promoCode.IsApplicableToPackage(packageId))
            throw BusinessException.PromoCodeNotApplicable(promoCode.Code);

        if (!promoCode.IsApplicableToOrder(orderAmount))
            throw BusinessException.PromoCodeMinimumAmountNotMet(promoCode.Code, promoCode.MinimumOrderAmount!.Value, orderAmount);
    }

    private static void ValidateSessionPromoCode(PromoCode promoCode, decimal amount)
    {
        if (!promoCode.IsActive)
            throw BusinessException.PromoCodeDeactivated(promoCode.Code);

        if (!promoCode.IsValidForDate())
            throw BusinessException.PromoCodeExpired(promoCode.Code, promoCode.EndDate);

        if (!promoCode.HasAvailableUsage())
            throw BusinessException.PromoCodeUsageLimitExceeded(promoCode.Code, promoCode.UsageLimit);

        if (!promoCode.IsApplicableToOrder(amount))
            throw BusinessException.PromoCodeMinimumAmountNotMet(promoCode.Code, promoCode.MinimumOrderAmount!.Value, amount);
    }

    #endregion
}
