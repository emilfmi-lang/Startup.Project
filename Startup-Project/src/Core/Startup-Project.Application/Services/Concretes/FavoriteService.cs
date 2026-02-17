using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Startup_Project.Application.Abstractions.UnitOfWork;
using Startup_Project.Application.Dtos.Common;
using Startup_Project.Application.Dtos.Favorites;
using Startup_Project.Application.Models;
using Startup_Project.Application.Services.Interfaces;
using Startup_Project.Domain.Abstractions.Repositories;
using Startup_Project.Domain.Entities;
using Startup_Project.Domain.Exceptions;
using System.Security.Claims;

namespace Startup_Project.Application.Services.Concretes;

public class FavoriteService : IFavoriteService
{
    private readonly IGenericRepository<FavoriteGym> _favoriteGymRepository;
    private readonly IGenericRepository<FavoriteTrainer> _favoriteTrainerRepository;
    private readonly IGenericRepository<Gym> _gymRepository;
    private readonly IGenericRepository<Trainer> _trainerRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUnitOfWork _unitOfWork;

    public FavoriteService(
        IGenericRepository<FavoriteGym> favoriteGymRepository,
        IGenericRepository<FavoriteTrainer> favoriteTrainerRepository,
        IGenericRepository<Gym> gymRepository,
        IGenericRepository<Trainer> trainerRepository,
        IHttpContextAccessor httpContextAccessor,
        IUnitOfWork unitOfWork)
    {
        _favoriteGymRepository = favoriteGymRepository;
        _favoriteTrainerRepository = favoriteTrainerRepository;
        _gymRepository = gymRepository;
        _trainerRepository = trainerRepository;
        _httpContextAccessor = httpContextAccessor;
        _unitOfWork = unitOfWork;
    }

    #region -------------------- PRIVATE HELPERS --------------------

    private Guid GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            throw BusinessException.UnauthorizedAccess();
        return userId;
    }

    private static FavoriteGymDto MapToFavoriteGymDto(FavoriteGym favorite)
    {
        var gym = favorite.Gym;
        var packages = gym.Packages.Where(p => !p.IsDeleted).ToList();
        var amenities = gym.Amenities.Where(a => !a.IsDeleted).OrderBy(a => a.DisplayOrder).ToList();

        // Max 3 amenity + "+N"
        var amenityTags = amenities.Take(3).Select(a => a.GetDisplayName()).ToList();
        if (amenities.Count > 3)
            amenityTags.Add($"+{amenities.Count - 3}");

        var minPrice = packages.Count != 0 ? packages.Min(p => p.Price) : 0;
        var maxPrice = packages.Count != 0 ? packages.Max(p => p.Price) : 0;

        return new FavoriteGymDto
        {
            Id = gym.Id,
            Name = gym.Name,
            Address = gym.Address,
            ImageUrl = gym.ImagePath,
            IsPremium = packages.Any(p => p.Price >= 80),
            IsOpen = gym.IsOpenNow(),
            StatusLabel = gym.IsOpenNow() ? "Open" : "Closed",
            Rating = gym.AverageRating,
            ReviewCount = gym.ReviewCount,
            RatingLabel = $"{gym.AverageRating:0.0} ({gym.ReviewCount} reviews)",
            Amenities = amenityTags,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            PriceLabel = minPrice == maxPrice
                ? $"?{minPrice:0}/mo"
                : $"?{minPrice:0} - ?{maxPrice:0}/mo",
            IsFavorite = true
        };
    }

    private static FavoriteTrainerDto MapToFavoriteTrainerDto(FavoriteTrainer favorite)
    {
        var trainer = favorite.Trainer;

        return new FavoriteTrainerDto
        {
            Id = trainer.Id,
            FullName = trainer.User.FullName,
            Specialization = trainer.Specialization,
            ProfileImageUrl = trainer.ProfileImageUrl,
            GymName = trainer.Gym?.Name,
            Rating = trainer.AverageRating,
            ReviewCount = trainer.ReviewCount,
            RatingLabel = $"{trainer.AverageRating:0.0} ({trainer.ReviewCount})",
            SessionPrice = trainer.SessionPrice,
            SessionPriceLabel = $"?{trainer.SessionPrice:0}/session",
            CompletedSessions = trainer.TotalSessions,
            SessionsLabel = $"{trainer.TotalSessions} sessions completed",
            IsFavorite = true
        };
    }

    #endregion

    #region -------------------- GYM FAVORITES --------------------

    public async Task<ResponseModel<bool>> ToggleGymFavoriteAsync(Guid gymId)
    {
        var userId = GetCurrentUserId();

        var gymExists = await _gymRepository.Table
            .AsNoTracking()
            .AnyAsync(g => g.Id == gymId && !g.IsDeleted);

        if (!gymExists)
            throw NotFoundException.Gym(gymId);

        var existingFavorite = await _favoriteGymRepository.Table
            .FirstOrDefaultAsync(f => f.UserId == userId && f.GymId == gymId);

        if (existingFavorite != null)
        {
            _favoriteGymRepository.Remove(existingFavorite);
            await _unitOfWork.SaveChangesAsync();
            return ResponseModel<bool>.SuccessResponse(false); // Silindi
        }

        var favorite = new FavoriteGym(userId, gymId);
        await _favoriteGymRepository.AddAsync(favorite);
        await _unitOfWork.SaveChangesAsync();

        return ResponseModel<bool>.SuccessResponse(true); // ?lav? edildi
    }

    public async Task<ResponseModel<PaginatedResult<FavoriteGymDto>>> GetFavoriteGymsAsync(int page = 1, int pageSize = 20)
    {
        var userId = GetCurrentUserId();

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var query = _favoriteGymRepository.Table
            .AsNoTracking()
            .Where(f => f.UserId == userId && !f.Gym.IsDeleted)
            .Include(f => f.Gym)
                .ThenInclude(g => g.OpeningHours)
            .Include(f => f.Gym)
                .ThenInclude(g => g.Packages.Where(p => !p.IsDeleted))
            .Include(f => f.Gym)
                .ThenInclude(g => g.Amenities.Where(a => !a.IsDeleted))
            .OrderByDescending(f => f.CreatedAt);

        var totalCount = await query.CountAsync();

        var favorites = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = favorites.Select(MapToFavoriteGymDto).ToList();

        var result = PaginatedResult<FavoriteGymDto>.Create(dtos, totalCount, page, pageSize);

        return ResponseModel<PaginatedResult<FavoriteGymDto>>.SuccessResponse(result);
    }

    public async Task<ResponseModel<bool>> IsGymFavoriteAsync(Guid gymId)
    {
        var userId = GetCurrentUserId();

        var isFavorite = await _favoriteGymRepository.Table
            .AsNoTracking()
            .AnyAsync(f => f.UserId == userId && f.GymId == gymId);

        return ResponseModel<bool>.SuccessResponse(isFavorite);
    }

    #endregion

    #region -------------------- TRAINER FAVORITES --------------------

    public async Task<ResponseModel<bool>> ToggleTrainerFavoriteAsync(Guid trainerId)
    {
        var userId = GetCurrentUserId();

        var trainerExists = await _trainerRepository.Table
            .AsNoTracking()
            .AnyAsync(t => t.Id == trainerId && !t.IsDeleted);

        if (!trainerExists)
            throw NotFoundException.Trainer(trainerId);

        var existingFavorite = await _favoriteTrainerRepository.Table
            .FirstOrDefaultAsync(f => f.UserId == userId && f.TrainerId == trainerId);

        if (existingFavorite != null)
        {
            _favoriteTrainerRepository.Remove(existingFavorite);
            await _unitOfWork.SaveChangesAsync();
            return ResponseModel<bool>.SuccessResponse(false); // Silindi
        }

        var favorite = new FavoriteTrainer(userId, trainerId);
        await _favoriteTrainerRepository.AddAsync(favorite);
        await _unitOfWork.SaveChangesAsync();

        return ResponseModel<bool>.SuccessResponse(true); // ?lav? edildi
    }

    public async Task<ResponseModel<PaginatedResult<FavoriteTrainerDto>>> GetFavoriteTrainersAsync(int page = 1, int pageSize = 20)
    {
        var userId = GetCurrentUserId();

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var query = _favoriteTrainerRepository.Table
            .AsNoTracking()
            .Where(f => f.UserId == userId && !f.Trainer.IsDeleted)
            .Include(f => f.Trainer)
                .ThenInclude(t => t.User)
            .Include(f => f.Trainer)
                .ThenInclude(t => t.Gym)
            .OrderByDescending(f => f.CreatedAt);

        var totalCount = await query.CountAsync();

        var favorites = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = favorites.Select(MapToFavoriteTrainerDto).ToList();

        var result = PaginatedResult<FavoriteTrainerDto>.Create(dtos, totalCount, page, pageSize);

        return ResponseModel<PaginatedResult<FavoriteTrainerDto>>.SuccessResponse(result);
    }

    public async Task<ResponseModel<bool>> IsTrainerFavoriteAsync(Guid trainerId)
    {
        var userId = GetCurrentUserId();

        var isFavorite = await _favoriteTrainerRepository.Table
            .AsNoTracking()
            .AnyAsync(f => f.UserId == userId && f.TrainerId == trainerId);

        return ResponseModel<bool>.SuccessResponse(isFavorite);
    }

    #endregion

    #region -------------------- COUNTS --------------------

    public async Task<ResponseModel<FavoriteCountsDto>> GetFavoriteCountsAsync()
    {
        var userId = GetCurrentUserId();

        var gymsCount = await _favoriteGymRepository.Table
            .AsNoTracking()
            .CountAsync(f => f.UserId == userId && !f.Gym.IsDeleted);

        var trainersCount = await _favoriteTrainerRepository.Table
            .AsNoTracking()
            .CountAsync(f => f.UserId == userId && !f.Trainer.IsDeleted);

        var counts = new FavoriteCountsDto
        {
            GymsCount = gymsCount,
            TrainersCount = trainersCount
        };

        return ResponseModel<FavoriteCountsDto>.SuccessResponse(counts);
    }

    #endregion
}
