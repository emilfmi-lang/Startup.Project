using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Startup_Project.Application.Dtos.PromoCode;
using Startup_Project.Application.Models;
using Startup_Project.Application.Services.Interfaces.Admin;
using Startup_Project.Domain.Abstractions.Repositories;
using Startup_Project.Domain.Entities;
using Startup_Project.Domain.Enums;
using Startup_Project.Domain.Exceptions;

namespace Startup_Project.Application.Services.Concretes.Admin;

public class PromoCodeAdminService(
    IGenericRepository<PromoCode> promoCodeRepository,
    IGenericRepository<PromoCodeUsage> usageRepository,
    IGenericRepository<Package> packageRepository,
    IValidator<CreatePromoCodeDto> createValidator,
    IValidator<UpdatePromoCodeDto> updateValidator) : IPromoCodeAdminService
{
    public async Task<ResponseModel<Guid>> CreateAsync(CreatePromoCodeDto dto)
    {
        var validationResult = await createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            throw BusinessException.ValidationFailed(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

        var normalizedCode = dto.Code.Trim().ToUpperInvariant();

        var exists = await promoCodeRepository.AnyAsync(p => p.Code == normalizedCode && !p.IsDeleted);
        if (exists)
            throw BusinessException.Custom(BusinessErrorCodes.ValidationFailed,
                $"'{normalizedCode}' kodlu promokod art?q mövcuddur.");

        if (dto.PackageId.HasValue)
        {
            var packageExists = await packageRepository.AnyAsync(p => p.Id == dto.PackageId.Value && !p.IsDeleted);
            if (!packageExists)
                throw NotFoundException.Package(dto.PackageId.Value);
        }

        var promoCode = new PromoCode(
            code: normalizedCode,
            description: dto.Description,
            discountType: dto.DiscountType,
            discountValue: dto.DiscountValue,
            startDate: dto.StartDate,
            endDate: dto.EndDate,
            usageLimit: dto.UsageLimit,
            minimumOrderAmount: dto.MinimumOrderAmount,
            packageId: dto.PackageId
        );

        await promoCodeRepository.AddAsync(promoCode);
        await promoCodeRepository.SaveAsync();

        return ResponseModel<Guid>.SuccessResponse(promoCode.Id);
    }

    public async Task<ResponseModel<List<PromoCodeListDto>>> GetAllAsync(int page = 1, int pageSize = 20)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var promoCodes = await promoCodeRepository.Table
            .Include(p => p.Package)
            .Where(p => !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();

        var dtos = promoCodes.Select(MapToListDto).ToList();

        return ResponseModel<List<PromoCodeListDto>>.SuccessResponse(dtos);
    }

    public async Task<ResponseModel<PromoCodeDetailDto>> GetByIdAsync(Guid id)
    {
        var promoCode = await promoCodeRepository.Table
            .Include(p => p.Package)
            .Include(p => p.PromoCodeUsages.OrderByDescending(u => u.UsedAt).Take(10))
                .ThenInclude(u => u.User)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        if (promoCode == null)
            throw NotFoundException.PromoCode(id.ToString());

        return ResponseModel<PromoCodeDetailDto>.SuccessResponse(MapToDetailDto(promoCode));
    }

    public async Task<ResponseModel<PromoCodeDetailDto>> GetByCodeAsync(string code)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();

        var promoCode = await promoCodeRepository.Table
            .Include(p => p.Package)
            .Include(p => p.PromoCodeUsages.OrderByDescending(u => u.UsedAt).Take(10))
                .ThenInclude(u => u.User)
            .FirstOrDefaultAsync(p => p.Code == normalizedCode && !p.IsDeleted);

        if (promoCode == null)
            throw NotFoundException.PromoCode(code);

        return ResponseModel<PromoCodeDetailDto>.SuccessResponse(MapToDetailDto(promoCode));
    }

    public async Task<ResponseModel<bool>> UpdateAsync(Guid id, UpdatePromoCodeDto dto)
    {
        var validationResult = await updateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            throw BusinessException.ValidationFailed(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

        var promoCode = await promoCodeRepository.GetByIdAsync(id);
        if (promoCode == null || promoCode.IsDeleted)
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
            if (dto.IsActive.Value)
                promoCode.Activate();
            else
                promoCode.Deactivate();
        }

        promoCodeRepository.Update(promoCode);
        await promoCodeRepository.SaveAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }

    public async Task<ResponseModel<bool>> DeactivateAsync(Guid id)
    {
        var promoCode = await promoCodeRepository.GetByIdAsync(id);
        if (promoCode == null || promoCode.IsDeleted)
            throw NotFoundException.PromoCode(id.ToString());

        promoCode.Deactivate();
        promoCodeRepository.Update(promoCode);
        await promoCodeRepository.SaveAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }

    public async Task<ResponseModel<bool>> ActivateAsync(Guid id)
    {
        var promoCode = await promoCodeRepository.GetByIdAsync(id);
        if (promoCode == null || promoCode.IsDeleted)
            throw NotFoundException.PromoCode(id.ToString());

        promoCode.Activate();
        promoCodeRepository.Update(promoCode);
        await promoCodeRepository.SaveAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }

    public async Task<ResponseModel<bool>> DeleteAsync(Guid id)
    {
        var promoCode = await promoCodeRepository.GetByIdAsync(id);
        if (promoCode == null || promoCode.IsDeleted)
            throw NotFoundException.PromoCode(id.ToString());

        promoCode.IsDeleted = true;
        promoCodeRepository.Update(promoCode);
        await promoCodeRepository.SaveAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }

    public async Task<ResponseModel<List<PromoCodeUsageDto>>> GetUsageHistoryAsync(Guid promoCodeId, int page = 1, int pageSize = 20)
    {
        var promoCodeExists = await promoCodeRepository.AnyAsync(p => p.Id == promoCodeId && !p.IsDeleted);
        if (!promoCodeExists)
            throw NotFoundException.PromoCode(promoCodeId.ToString());

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var usages = await usageRepository.Table
            .Include(u => u.User)
            .Where(u => u.PromoCodeId == promoCodeId)
            .OrderByDescending(u => u.UsedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();

        var dtos = usages.Select(u => new PromoCodeUsageDto
        {
            Id = u.Id,
            UserId = u.UserId,
            UserFullName = u.User.FullName,
            UserEmail = u.User.Email ?? "",
            PackageId = u.PackageId,
            PackageName = "",
            AppliedDiscount = u.AppliedDiscount,
            UsedAt = u.UsedAt
        }).ToList();

        return ResponseModel<List<PromoCodeUsageDto>>.SuccessResponse(dtos);
    }

    // ????????????????????????????????????????????????????????????????
    // PRIVATE HELPER METHODS
    // ????????????????????????????????????????????????????????????????

    private static PromoCodeListDto MapToListDto(PromoCode p) => new()
    {
        Id = p.Id,
        Code = p.Code,
        Description = p.Description,
        DiscountType = p.DiscountType.ToString(),
        DiscountValue = p.DiscountValue,
        UsageLimit = p.UsageLimit,
        UsedCount = p.UsedCount,
        StartDate = p.StartDate,
        EndDate = p.EndDate,
        IsActive = p.IsActive,
        PackageName = p.Package?.Name
    };

    private static PromoCodeDetailDto MapToDetailDto(PromoCode promoCode) => new()
    {
        Id = promoCode.Id,
        Code = promoCode.Code,
        Description = promoCode.Description,
        DiscountType = promoCode.DiscountType.ToString(),
        DiscountValue = promoCode.DiscountValue,
        MinimumOrderAmount = promoCode.MinimumOrderAmount,
        UsageLimit = promoCode.UsageLimit,
        UsedCount = promoCode.UsedCount,
        StartDate = promoCode.StartDate,
        EndDate = promoCode.EndDate,
        IsActive = promoCode.IsActive,
        PackageId = promoCode.PackageId,
        PackageName = promoCode.Package?.Name,
        CreatedAt = promoCode.CreatedAt,
        UpdatedAt = promoCode.UpdatedAt,
        RecentUsages = promoCode.PromoCodeUsages.Select(u => new PromoCodeUsageDto
        {
            Id = u.Id,
            UserId = u.UserId,
            UserFullName = u.User?.FullName ?? "",
            UserEmail = u.User?.Email ?? "",
            PackageId = u.PackageId,
            PackageName = "",
            AppliedDiscount = u.AppliedDiscount,
            UsedAt = u.UsedAt
        }).ToList()
    };
}
