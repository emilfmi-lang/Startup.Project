using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Startup_Project.Application.Dtos.PromoCode;
using Startup_Project.Application.Models;
using Startup_Project.Application.Services.Interfaces;
using Startup_Project.Domain.Abstractions.Repositories;
using Startup_Project.Domain.Entities;
using Startup_Project.Domain.Exceptions;
using System.Security.Claims;

namespace Startup_Project.Application.Services.Concretes;

public class PromoCodeService(
    IGenericRepository<PromoCode> promoCodeRepository,
    IGenericRepository<PromoCodeUsage> usageRepository,
    IGenericRepository<Package> packageRepository,
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

    public async Task<ResponseModel<PromoCodeResultDto>> ValidateAsync(string code, Guid packageId)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();

        var promoCode = await promoCodeRepository.Table
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Code == normalizedCode && !p.IsDeleted);

        if (promoCode == null)
            throw NotFoundException.PromoCode(code);

        var package = await packageRepository.GetByIdAsync(packageId);
        if (package == null)
            throw NotFoundException.Package(packageId);

        ValidatePromoCode(promoCode, packageId, package.Price);

        var discountAmount = promoCode.CalculateDiscount(package.Price);
        var finalPrice = package.Price - discountAmount;

        return ResponseModel<PromoCodeResultDto>.SuccessResponse(new PromoCodeResultDto
        {
            IsValid = true,
            OriginalPrice = package.Price,
            DisCountAmount = discountAmount,
            FinalPrice = finalPrice,
            DiscountType = promoCode.DiscountType.ToString(),
            DiscountValue = promoCode.DiscountValue,
            Message = $"Promokod keçerlidir. {discountAmount:F2} AZN endirim t?tbiq olunacaq."
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

        var package = await packageRepository.GetByIdAsync(dto.PackageId);
        if (package == null)
            throw NotFoundException.Package(dto.PackageId);

        var alreadyUsed = await usageRepository.AnyAsync(
            u => u.PromoCodeId == promoCode.Id && u.UserId == userId);

        if (alreadyUsed)
            throw BusinessException.PromoCodeAlreadyUsed(dto.Code);

        ValidatePromoCode(promoCode, dto.PackageId, package.Price);

        var discountAmount = promoCode.CalculateDiscount(package.Price);
        var finalPrice = package.Price - discountAmount;

        try
        {
            promoCode.IncrementUsedCount();
            promoCodeRepository.Update(promoCode);

            var usage = new PromoCodeUsage(promoCode.Id, userId, dto.PackageId, discountAmount);
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
            DisCountAmount = discountAmount,
            FinalPrice = finalPrice,
            DiscountType = promoCode.DiscountType.ToString(),
            DiscountValue = promoCode.DiscountValue,
            Message = $"Promokod u?urla t?tbiq edildi. {discountAmount:F2} AZN endirim."
        });
    }

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
}
