using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Startup_Project.Application.Dtos.GymDtos;
using Startup_Project.Application.Models;
using Startup_Project.Application.Services.Interfaces;
using Startup_Project.Domain.Abstractions.Repositories;
using Startup_Project.Domain.Entities;
using Startup_Project.Domain.Exceptions;
using System.Security.Claims;

namespace Startup_Project.Application.Services.Concretes;

public class GymService(
    IGenericRepository<Gym> repository,
    IGenericRepository<FavoriteGym> favoriteRepository,
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
            throw BusinessException.UnauthorizedAccess();

        var gym = new Gym(dto.Name, dto.Address, userId.Value);

        await repository.AddAsync(gym);
        await repository.SaveAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }

    public async Task<ResponseModel<bool>> UpdateAsync(string id, GymUpdateDto dto)
    {
        if (!Guid.TryParse(id, out var gymGuid))
            throw BusinessException.ValidationFailed("Zal ID-si yanlışdır.");

        var gym = await repository.GetByIdAsync(gymGuid);
        if (gym == null || gym.IsDeleted)
            throw NotFoundException.Gym(gymGuid);

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

    public async Task<ResponseModel<List<GymListDto>>> GetAllWithFavoritesAsync(int page = 1, int pageSize = 20)
    {
        var userId = GetCurrentUserId();

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var userFavoriteGymIds = new HashSet<Guid>();
        if (userId != null)
        {
            userFavoriteGymIds = (await favoriteRepository.Table
                .AsNoTracking()
                .Where(f => f.UserId == userId.Value)
                .Select(f => f.GymId)
                .ToListAsync())
                .ToHashSet();
        }

        var gyms = await repository.Table
            .AsNoTracking()
            .Where(g => !g.IsDeleted)
            .Include(g => g.OpeningHours)
            .Include(g => g.FavoritedByUsers)
            .OrderByDescending(g => g.AverageRating)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Map to DTOs with IsFavorite
        var gymDtos = gyms.Select(g => new GymListDto
        {
            Id = g.Id,
            Name = g.Name,
            Address = g.Address,
            ImagePath = g.ImagePath,
            AverageRating = g.AverageRating,
            ReviewCount = g.ReviewCount,
            IsOpen = g.IsOpenNow(),
            IsFavorite = userFavoriteGymIds.Contains(g.Id),
            FavoriteCount = g.FavoritedByUsers.Count
        }).ToList();

        return ResponseModel<List<GymListDto>>.SuccessResponse(gymDtos);
    }

    public async Task<ResponseModel<bool>> DeleteAsync(string id)
    {
        if (!Guid.TryParse(id, out var gymGuid))
            throw BusinessException.ValidationFailed("Zal ID-si yanlışdır.");

        var gym = await repository.GetByIdAsync(gymGuid);
        if (gym == null || gym.IsDeleted)
            throw NotFoundException.Gym(gymGuid);

        gym.IsDeleted = true;
        repository.Update(gym);
        await repository.SaveAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }
}
