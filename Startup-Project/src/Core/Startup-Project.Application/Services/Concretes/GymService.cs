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
                        IMapper mapper,
                        IHttpContextAccessor httpContextAccessor) : IGymService
{
    private bool IsAdmin()
    {
        var roles = httpContextAccessor.HttpContext?.User.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList() ?? [];
        return roles.Contains("Admin");
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return null;
        return userId;
    }

    public async Task<ResponseModel<bool>> CreateAsync(GymCreateDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return ResponseModel<bool>.FailureResponse("Zal yaratmaq ucun giris etmelisiniz.");

        var gym = new Gym(dto.Name, dto.Address, userId.Value);

        await repository.AddAsync(gym);
        await repository.SaveAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }

    public async Task<ResponseModel<bool>> UpdateAsync(string id, GymUpdateDto dto)
    {
        if (!Guid.TryParse(id, out var gymGuid))
            return ResponseModel<bool>.FailureResponse("Zal ID-si yanlisdir.");

        var gym = await repository.GetByIdAsync(gymGuid);
        if (gym == null || gym.IsDeleted)
            return ResponseModel<bool>.FailureResponse("Zal tapilmadi.");

        if (!string.IsNullOrWhiteSpace(dto.Name))
            gym.SetName(dto.Name);

        if (!string.IsNullOrWhiteSpace(dto.Address))
            gym.SetAddress(dto.Address);

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
            return ResponseModel<bool>.FailureResponse("Zal ID-si yanlisdir.");

        var gym = await repository.GetByIdAsync(gymGuid);
        if (gym == null || gym.IsDeleted)
            return ResponseModel<bool>.FailureResponse("Zal tapilmadi.");

        gym.IsDeleted = true;
        repository.Update(gym);
        await repository.SaveAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }
}
