using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Startup_Project.Application.Abstractions.UnitOfWork;
using Startup_Project.Application.Dtos.GymDtos;
using Startup_Project.Application.Models;
using Startup_Project.Application.Services.Interfaces;
using Startup_Project.Domain.Abstractions.Repositories;
using Startup_Project.Domain.Entities;
using Startup_Project.Domain.Exceptions;
using System.Security.Claims;

namespace Startup_Project.Application.Services.Concretes;

public class FavoriteService : IFavoriteService
{
    private readonly IGenericRepository<FavoriteGym> _favoriteRepository;
    private readonly IGenericRepository<Gym> _gymRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUnitOfWork _unitOfWork;

    public FavoriteService(
        IGenericRepository<FavoriteGym> favoriteRepository,
        IGenericRepository<Gym> gymRepository,
        IHttpContextAccessor httpContextAccessor,
        IUnitOfWork unitOfWork)
    {
        _favoriteRepository = favoriteRepository;
        _gymRepository = gymRepository;
        _httpContextAccessor = httpContextAccessor;
        _unitOfWork = unitOfWork;
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    public async Task<ResponseModel<bool>> ToggleFavoriteAsync(Guid gymId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            throw BusinessException.UnauthorizedAccess();

        var gymExists = await _gymRepository.Table
            .AsNoTracking()
            .AnyAsync(g => g.Id == gymId && !g.IsDeleted);

        if (!gymExists)
            throw NotFoundException.Gym(gymId);

        // Check if already favorited
        var existingFavorite = await _favoriteRepository.Table
            .FirstOrDefaultAsync(f => f.UserId == userId.Value && f.GymId == gymId);

        if (existingFavorite != null)
        {
            // Remove from favorites
            _favoriteRepository.Remove(existingFavorite);
            await _unitOfWork.SaveChangesAsync();
            return ResponseModel<bool>.SuccessResponse(false); // Silindi
        }

        // Add to favorites
        var favorite = new FavoriteGym(userId.Value, gymId);
        await _favoriteRepository.AddAsync(favorite);
        await _unitOfWork.SaveChangesAsync();

        return ResponseModel<bool>.SuccessResponse(true); // ?lav? edildi
    }

    public async Task<ResponseModel<List<GymListDto>>> GetUserFavoritesAsync(int page = 1, int pageSize = 20)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            throw BusinessException.UnauthorizedAccess();

        // Validate pagination
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var favorites = await _favoriteRepository.Table
            .AsNoTracking()
            .Where(f => f.UserId == userId.Value && !f.Gym.IsDeleted)
            .Include(f => f.Gym)
                .ThenInclude(g => g.OpeningHours)
            .OrderByDescending(f => f.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => new GymListDto
            {
                Id = f.Gym.Id,
                Name = f.Gym.Name,
                Address = f.Gym.Address,
                ImagePath = f.Gym.ImagePath,
                AverageRating = f.Gym.AverageRating,
                ReviewCount = f.Gym.ReviewCount,
                IsOpen = f.Gym.IsOpenNow(),
                IsFavorite = true, // Bu list art?q favorit olanlar? g?tirir
                FavoriteCount = f.Gym.FavoritedByUsers.Count
            })
            .ToListAsync();

        return ResponseModel<List<GymListDto>>.SuccessResponse(favorites);
    }

    public async Task<ResponseModel<bool>> IsFavoriteAsync(Guid gymId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return ResponseModel<bool>.SuccessResponse(false); // Anonim istifad?çi üçün false

        var isFavorite = await _favoriteRepository.Table
            .AsNoTracking()
            .AnyAsync(f => f.UserId == userId.Value && f.GymId == gymId);

        return ResponseModel<bool>.SuccessResponse(isFavorite);
    }

    public async Task<ResponseModel<int>> GetFavoriteCountAsync(Guid gymId)
    {
        var count = await _favoriteRepository.Table
            .AsNoTracking()
            .CountAsync(f => f.GymId == gymId);

        return ResponseModel<int>.SuccessResponse(count);
    }
}
