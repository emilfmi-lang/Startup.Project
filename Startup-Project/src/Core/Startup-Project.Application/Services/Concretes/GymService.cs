using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Startup_Project.Application.Dtos.GymDtos;
using Startup_Project.Application.Models;
using Startup_Project.Application.Services.Interfaces;
using Startup_Project.Domain.Abstractions.Repositories;
using Startup_Project.Domain.Entities;
using System.Security.Claims;

namespace Startup_Project.Application.Services.Concretes;

public class GymService(IGenericRepository<Gym> repository,
                        IFileService fileService,
                        IMapper mapper,
                        IHttpContextAccessor httpContextAccessor) : IGymService
{
    public async Task<ResponseModel<bool>> CreateAsync(GymCreateDto dto)
    {
        var userId = httpContextAccessor.HttpContext?
            .User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userId))
            return ResponseModel<bool>.FailureResponse("Zal yaratmaq üçün qeydiyyatdan keçməlisiniz.");

        if (!Guid.TryParse(userId, out var ownerGuid))
            return ResponseModel<bool>.FailureResponse("İstifadəçi məlumatı yanlışdır.");

        string? imagePath = null;

        if (dto.Image != null)
            imagePath = await fileService.UploadFileAsync(dto.Image, "gyms");

        var gym = new Gym(dto.Name, dto.Address, ownerGuid, imagePath);

        await repository.AddAsync(gym);
        await repository.SaveAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }

    public async Task<ResponseModel<bool>> UpdateAsync(string id, GymUpdateDto dto)
    {
        if (!Guid.TryParse(id, out var gymGuid))
            return ResponseModel<bool>.FailureResponse("Zal ID-si yanlışdır.");

        var gym = await repository.GetByIdAsync(gymGuid);

        if (gym == null)
            return ResponseModel<bool>.FailureResponse("Zal tapılmadı.");

        var userId = httpContextAccessor.HttpContext?
            .User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return ResponseModel<bool>.FailureResponse("İstifadəçi təyin edilməyib.");

        if (!Guid.TryParse(userId, out var ownerGuid))
            return ResponseModel<bool>.FailureResponse("İstifadəçi məlumatı yanlışdır.");

        if (gym.OwnerId != ownerGuid)
            return ResponseModel<bool>.FailureResponse("Bu zalı yalnız özünüz yeniləyə bilərsiniz.");

        if (!string.IsNullOrWhiteSpace(dto.Name))
            gym.SetName(dto.Name);

        if (!string.IsNullOrWhiteSpace(dto.Address))
            gym.SetAddress(dto.Address);

        if (dto.NewImage != null)
        {
            if (!string.IsNullOrEmpty(gym.ImagePath))
                await fileService.DeleteFileAsync(gym.ImagePath);

            var newImagePath = await fileService.UploadFileAsync(dto.NewImage, "gyms");
            gym.UpdateImage(newImagePath);
        }

        repository.Update(gym);
        await repository.SaveAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }

    public async Task<ResponseModel<List<GymGetDto>>> GetAllAsync()
    {
        var gyms = await repository.Table
            .Include(g => g.Owner)
            .Where(g => !g.IsDeleted)
            .AsNoTracking()
            .ToListAsync();

        var gymDtos = mapper.Map<List<GymGetDto>>(gyms);

        return ResponseModel<List<GymGetDto>>.SuccessResponse(gymDtos);
    }

    public async Task<ResponseModel<bool>> DeleteAsync(string id)
    {
        if (!Guid.TryParse(id, out var gymGuid))
            return ResponseModel<bool>.FailureResponse("Zal ID-si yanlışdır.");

        var gym = await repository.GetByIdAsync(gymGuid);

        if (gym == null || gym.IsDeleted)
            return ResponseModel<bool>.FailureResponse("Zal tapılmadı.");

        var userId = httpContextAccessor.HttpContext?
            .User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return ResponseModel<bool>.FailureResponse("İstifadəçi təyin edilməyib.");

        if (!Guid.TryParse(userId, out var ownerGuid))
            return ResponseModel<bool>.FailureResponse("İstifadəçi məlumatı yanlışdır.");

        if (gym.OwnerId != ownerGuid)
            return ResponseModel<bool>.FailureResponse("Bu zalı yalnız özünüz silə bilərsiniz.");

        if (!string.IsNullOrEmpty(gym.ImagePath))
            await fileService.DeleteFileAsync(gym.ImagePath);

        gym.IsDeleted = true;
        repository.Update(gym);
        await repository.SaveAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }
}
