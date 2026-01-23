using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Startup_Project.Application.Abstractions.Services;
using Startup_Project.Application.Dtos.GymDtos;
using Startup_Project.Application.Models;
using Startup_Project.Application.Services.Interfaces;
using Startup_Project.Application.Validators.Gym;
using Startup_Project.Domain.Abstractions.Repositories;
using Startup_Project.Domain.Entities;
using System.Security.Claims;


namespace Startup_Project.Application.Services.Concretes;

public class GymService(IGenericRepository<Gym> repository,
                        IFileService fileService,
                        IMapper mapper,
                         IHttpContextAccessor httpContextAccessor ) : IGymService
{
    public async Task<ResponseModel<bool>> CreateAsync(GymCreateDto dto)
    {
        var validator = new GymCreateDtoValidator();
        var validationResult = await validator.ValidateAsync(dto);

        if (!validationResult.IsValid)
        {
            return ResponseModel<bool>.FailureResponse(validationResult.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var userId = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return ResponseModel<bool>.FailureResponse("İstifadəçi təyin edilməyib.");
        }

        string? imagePath = null;
        if (dto.Image != null)
        {
            imagePath = await fileService.UploadFileAsync(dto.Image, "gyms");
        }

        var gym = new Gym(dto.Name, dto.Address, Guid.Parse(userId), imagePath);

        var result = await repository.AddAsync(gym);
        if (!result)
        {
            return ResponseModel<bool>.FailureResponse("Zal yaradılarkən xəta baş verdi.");
        }

        await repository.SaveAsync();
        return ResponseModel<bool>.SuccessResponse(true);
    }

    public async Task<ResponseModel<bool>> UpdateAsync(string id, GymUpdateDto dto)
    {
        var validator = new GymUpdateDtoValidator();
        var validationResult = await validator.ValidateAsync(dto);

        if (!validationResult.IsValid)
        {
            return ResponseModel<bool>.FailureResponse(validationResult.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var gym = await repository.GetByIdAsync(id, tracking: true);
        if (gym == null)
        {
            return ResponseModel<bool>.FailureResponse("Zal tapılmadı.");
        }

        var userId = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || gym.OwnerId != Guid.Parse(userId))
        {
            return ResponseModel<bool>.FailureResponse("Bu əməliyyatı yerinə yetirmək üçün icazəniz yoxdur.");
        }

        gym.SetName(dto.Name);
        gym.SetAddress(dto.Address);

        if (dto.NewImage != null)
        {
            var newImagePath = await fileService.UploadFileAsync(dto.NewImage, "gyms");
            gym.UpdateImage(newImagePath);
        }

        repository.Update(gym);
        await repository.SaveAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }

    public async Task<ResponseModel<List<GymGetDto>>> GetAllAsync()
    {
        var gyms = await repository.Table.Include(g => g.Owner).AsNoTracking().ToListAsync();
        var gymDtos = mapper.Map<List<GymGetDto>>(gyms);
        return ResponseModel<List<GymGetDto>>.SuccessResponse(gymDtos);
    }

    public async Task<ResponseModel<bool>> DeleteAsync(string id)
    {
        var gym = await repository.GetByIdAsync(id, tracking: true);
        if (gym == null)
        {
            return ResponseModel<bool>.FailureResponse("Zal tapılmadı.");
        }

        var userId = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || gym.OwnerId != Guid.Parse(userId))
        {
            return ResponseModel<bool>.FailureResponse("Bu əməliyyatı yerinə yetirmək üçün icazəniz yoxdur.");
        }
        if(!string.IsNullOrEmpty(gym.ImagePath))
        {
            await fileService.DeleteFileAsync(gym.ImagePath);
        }

        repository.Remove(gym);
        await repository.SaveAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }
}
