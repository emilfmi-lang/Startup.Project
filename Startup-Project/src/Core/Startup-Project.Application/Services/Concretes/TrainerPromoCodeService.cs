using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Startup_Project.Application.Dtos.PromoCode;
using Startup_Project.Application.Models;
using Startup_Project.Application.Services.Interfaces;
using Startup_Project.Domain.Abstractions.Repositories;
using Startup_Project.Domain.Entities;
using Startup_Project.Domain.Enums;
using Startup_Project.Domain.Enums.Errors;
using Startup_Project.Domain.Exceptions;
using System.Security.Claims;

namespace Startup_Project.Application.Services.Concretes;

public class TrainerPromoCodeService(
    IGenericRepository<PromoCode> promoCodeRepository,
    IGenericRepository<Trainer> trainerRepository,
    IHttpContextAccessor httpContextAccessor,
    IValidator<TrainerCreatePromoCodeDto> createValidator,
    IValidator<UpdatePromoCodeDto> updateValidator) : ITrainerPromoCodeService
{
    private Guid GetCurrentUserId()
    {
        var userId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            throw BusinessException.UnauthorizedAccess();
        return userGuid;
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

    public async Task<ResponseModel<Guid>> CreateAsync(TrainerCreatePromoCodeDto dto)
    {
        var validationResult = await createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            throw BusinessException.ValidationFailed(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

        var trainer = await GetCurrentTrainerAsync();
        var normalizedCode = dto.Code.Trim().ToUpperInvariant();

        var exists = await promoCodeRepository.AnyAsync(p => p.Code == normalizedCode && !p.IsDeleted);
        if (exists)
            throw BusinessException.Custom(BusinessErrorCodes.ValidationFailed,
                $"'{normalizedCode}' kodlu promokod artiq movcuddur.");

        var promoCode = new PromoCode(
            code: normalizedCode,
            description: dto.Description,
            discountType: dto.DiscountType,
            discountValue: dto.DiscountValue,
            targetType: PromoCodeTargetType.Session,
            startDate: dto.StartDate,
            endDate: dto.EndDate,
            usageLimit: dto.UsageLimit,
            createdById: trainer.UserId,
            minimumOrderAmount: dto.MinimumOrderAmount,
            trainerId: trainer.Id
        );

        await promoCodeRepository.AddAsync(promoCode);
        await promoCodeRepository.SaveAsync();

        return ResponseModel<Guid>.SuccessResponse(promoCode.Id);
    }

    public async Task<ResponseModel<List<PromoCodeListDto>>> GetMyPromoCodesAsync(int page = 1, int pageSize = 20)
    {
        var trainer = await GetCurrentTrainerAsync();

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var promoCodes = await promoCodeRepository.Table
            .Where(p => p.TrainerId == trainer.Id && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();

        var dtos = promoCodes.Select(p => new PromoCodeListDto
        {
            Id = p.Id,
            Code = p.Code,
            Description = p.Description,
            DiscountType = p.DiscountType.ToString(),
            DiscountValue = p.DiscountValue,
            TargetType = p.TargetType.ToString(),
            UsageLimit = p.UsageLimit,
            UsedCount = p.UsedCount,
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            IsActive = p.IsActive
        }).ToList();

        return ResponseModel<List<PromoCodeListDto>>.SuccessResponse(dtos);
    }

    public async Task<ResponseModel<PromoCodeDetailDto>> GetByIdAsync(Guid id)
    {
        var trainer = await GetCurrentTrainerAsync();

        var promoCode = await promoCodeRepository.Table
            .Include(p => p.PromoCodeUsages.OrderByDescending(u => u.UsedAt).Take(10))
                .ThenInclude(u => u.User)
            .Include(p => p.CreatedBy)
            .FirstOrDefaultAsync(p => p.Id == id && p.TrainerId == trainer.Id && !p.IsDeleted);

        if (promoCode == null)
            throw NotFoundException.PromoCode(id.ToString());

        return ResponseModel<PromoCodeDetailDto>.SuccessResponse(new PromoCodeDetailDto
        {
            Id = promoCode.Id,
            Code = promoCode.Code,
            Description = promoCode.Description,
            DiscountType = promoCode.DiscountType.ToString(),
            DiscountValue = promoCode.DiscountValue,
            TargetType = promoCode.TargetType.ToString(),
            MinimumOrderAmount = promoCode.MinimumOrderAmount,
            UsageLimit = promoCode.UsageLimit,
            UsedCount = promoCode.UsedCount,
            StartDate = promoCode.StartDate,
            EndDate = promoCode.EndDate,
            IsActive = promoCode.IsActive,
            CreatedById = promoCode.CreatedById,
            CreatedByName = promoCode.CreatedBy?.FullName,
            TrainerId = promoCode.TrainerId,
            CreatedAt = promoCode.CreatedAt,
            UpdatedAt = promoCode.UpdatedAt,
            RecentUsages = promoCode.PromoCodeUsages.Select(u => new PromoCodeUsageDto
            {
                Id = u.Id,
                UserId = u.UserId,
                UserFullName = u.User?.FullName ?? "",
                UserEmail = u.User?.Email ?? "",
                PackageId = u.PackageId ?? Guid.Empty,
                PackageName = "",
                AppliedDiscount = u.AppliedDiscount,
                UsedAt = u.UsedAt
            }).ToList()
        });
    }

    public async Task<ResponseModel<bool>> UpdateAsync(Guid id, UpdatePromoCodeDto dto)
    {
        var trainer = await GetCurrentTrainerAsync();

        var validationResult = await updateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            throw BusinessException.ValidationFailed(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

        var promoCode = await promoCodeRepository.GetByIdAsync(id);
        if (promoCode == null || promoCode.IsDeleted || promoCode.TrainerId != trainer.Id)
            throw NotFoundException.PromoCode(id.ToString());

        promoCode.Update(
            description: dto.Description,
            discountValue: dto.DiscountValue,
            minimumOrderAmount: dto.MinimumOrderAmount,
            usageLimit: dto.UsageLimit,
            startDate: dto.StartDate,
            endDate: dto.EndDate
        );

        if (dto.IsActive.HasValue)
        {
            if (dto.IsActive.Value) promoCode.Activate();
            else promoCode.Deactivate();
        }

        promoCodeRepository.Update(promoCode);
        await promoCodeRepository.SaveAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }

    public async Task<ResponseModel<bool>> DeactivateAsync(Guid id)
    {
        var trainer = await GetCurrentTrainerAsync();

        var promoCode = await promoCodeRepository.GetByIdAsync(id);
        if (promoCode == null || promoCode.IsDeleted || promoCode.TrainerId != trainer.Id)
            throw NotFoundException.PromoCode(id.ToString());

        promoCode.Deactivate();
        promoCodeRepository.Update(promoCode);
        await promoCodeRepository.SaveAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }

    public async Task<ResponseModel<bool>> ActivateAsync(Guid id)
    {
        var trainer = await GetCurrentTrainerAsync();

        var promoCode = await promoCodeRepository.GetByIdAsync(id);
        if (promoCode == null || promoCode.IsDeleted || promoCode.TrainerId != trainer.Id)
            throw NotFoundException.PromoCode(id.ToString());

        promoCode.Activate();
        promoCodeRepository.Update(promoCode);
        await promoCodeRepository.SaveAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }

    public async Task<ResponseModel<bool>> DeleteAsync(Guid id)
    {
        var trainer = await GetCurrentTrainerAsync();

        var promoCode = await promoCodeRepository.GetByIdAsync(id);
        if (promoCode == null || promoCode.IsDeleted || promoCode.TrainerId != trainer.Id)
            throw NotFoundException.PromoCode(id.ToString());

        promoCode.IsDeleted = true;
        promoCodeRepository.Update(promoCode);
        await promoCodeRepository.SaveAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }
}
