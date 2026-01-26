using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Startup_Project.Application.Dtos.Packages;
using Startup_Project.Application.Models;
using Startup_Project.Application.Services.Interfaces;
using Startup_Project.Domain.Abstractions.Repositories;
using Startup_Project.Domain.Entities;

namespace Startup_Project.Application.Services.Concretes;

public class PackageService(IGenericRepository<Package> repository,
                             IMapper mapper,
                             IValidator<CreatePackageRequestDto> createValidator,
                             IValidator<UpdatePackageRequestDto> updateValidator) : IPackageService
{
    public async Task<ResponseModel<bool>> CreateAsync(CreatePackageRequestDto dto)
    {
        var validationResult = await createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return ResponseModel<bool>.FailureResponse(errors);
        }

        var package = new Package(dto.Name, dto.Price, dto.DurationInDays, dto.GymId);

        await repository.AddAsync(package);
        await repository.SaveAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }

    public async Task<ResponseModel<bool>> UpdateAsync(string id, UpdatePackageRequestDto dto)
    {
        if (!Guid.TryParse(id, out var packageGuid))
            return ResponseModel<bool>.FailureResponse("Paket ID-si yanlışdır.");

        var validationResult = await updateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return ResponseModel<bool>.FailureResponse(errors);
        }

        var package = await repository.GetByIdAsync(packageGuid);

        if (package == null || package.IsDeleted)
            return ResponseModel<bool>.FailureResponse("Paket tapılmadı.");

        package.SetName(dto.Name);
        package.SetPrice(dto.Price);
        package.SetDurationInDays(dto.DurationInDays);
        package.SetGymId(dto.GymId);

        repository.Update(package);
        await repository.SaveAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }

    public async Task<ResponseModel<bool>> DeleteAsync(string id)
    {
        if (!Guid.TryParse(id, out var packageGuid))
            return ResponseModel<bool>.FailureResponse("Paket ID-si yanlışdır.");

        var package = await repository.GetByIdAsync(packageGuid);

        if (package == null || package.IsDeleted)
            return ResponseModel<bool>.FailureResponse("Paket tapılmadı.");

        package.IsDeleted = true;
        repository.Update(package);
        await repository.SaveAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }

    public async Task<ResponseModel<List<PackageResponseDto>>> GetAllAsync()
    {
        var packages = await repository.Table
            .Include(p => p.Gym)
            .Where(p => !p.IsDeleted)
            .AsNoTracking()
            .ToListAsync();

        var packageDtos = mapper.Map<List<PackageResponseDto>>(packages);

        return ResponseModel<List<PackageResponseDto>>.SuccessResponse(packageDtos);
    }

    public async Task<ResponseModel<PackageResponseDto>> GetByIdAsync(string id)
    {
        if (!Guid.TryParse(id, out var packageGuid))
            return ResponseModel<PackageResponseDto>.FailureResponse("Paket ID-si yanlışdır.");

        var package = await repository.Table
            .Include(p => p.Gym)
            .Where(p => p.Id == packageGuid && !p.IsDeleted)
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (package == null)
            return ResponseModel<PackageResponseDto>.FailureResponse("Paket tapılmadı.");

        var packageDto = mapper.Map<PackageResponseDto>(package);

        return ResponseModel<PackageResponseDto>.SuccessResponse(packageDto);
    }
}
