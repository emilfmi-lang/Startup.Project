using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Startup_Project.Application.Dtos.GymDtos;
using Startup_Project.Application.Dtos.GymDtos.GymDetail;
using Startup_Project.Application.Dtos.GymOpeningHour;
using Startup_Project.Application.Dtos.Packages;
using Startup_Project.Application.Dtos.Trainer;
using Startup_Project.Application.Models;
using Startup_Project.Application.Services.Interfaces;
using Startup_Project.Domain.Abstractions.Repositories;
using Startup_Project.Domain.Entities;
using Startup_Project.Domain.Enums;
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
            .Include(g => g.Amenities.Where(a => a.IsHighlighted && !a.IsDeleted))
            .Include(g => g.Packages.Where(p => !p.IsDeleted))
            .OrderByDescending(g => g.AverageRating)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

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
            FavoriteCount = g.FavoritedByUsers.Count,
            Tags = g.Amenities
                .Where(a => a.IsHighlighted && !a.IsDeleted)
                .OrderBy(a => a.DisplayOrder)
                .Take(4)
                .Select(a => a.AmenityType.ToString())
                .ToList(),
            MinMonthlyPrice = g.Packages
                .Where(p => !p.IsDeleted)
                .OrderBy(p => p.Price)
                .Select(p => (decimal?)p.Price)
                .FirstOrDefault()
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

        public async Task<ResponseModel<GymDetailDto>> GetDetailAsync(Guid gymId)
        {
            var userId = GetCurrentUserId();

            var gym = await repository.Table
                .AsNoTracking()
                .Where(g => g.Id == gymId && !g.IsDeleted)
                .Include(g => g.Images.Where(i => !i.IsDeleted).OrderBy(i => i.DisplayOrder))
                .Include(g => g.OpeningHours)
                .Include(g => g.Amenities.Where(a => !a.IsDeleted))
                .Include(g => g.Packages.Where(p => !p.IsDeleted))
                    .ThenInclude(p => p.Features.Where(f => !f.IsDeleted).OrderBy(f => f.DisplayOrder))
                .Include(g => g.Trainers.Where(t => !t.IsDeleted && t.Status == TrainerStatus.Active))
                    .ThenInclude(t => t.User)
                .Include(g => g.FavoritedByUsers)
                .FirstOrDefaultAsync();

            if (gym == null)
                throw NotFoundException.Gym(gymId);

            var isFavorite = userId.HasValue && gym.FavoritedByUsers.Any(f => f.UserId == userId.Value);

            var mainImage = gym.Images.FirstOrDefault(i => i.IsMain)?.ImagePath ?? gym.ImagePath;

            var dto = new GymDetailDto
            {
                Id = gym.Id,
                Name = gym.Name,
                Address = gym.Address,
                Description = gym.Description,

                MainImage = mainImage,
                Images = gym.Images.Select(i => new GymImageDto
                {
                    Id = i.Id,
                    ImagePath = i.ImagePath,
                    Alt = i.Alt,
                    DisplayOrder = i.DisplayOrder,
                    IsMain = i.IsMain
                }).ToList(),

                AverageRating = gym.AverageRating,
                ReviewCount = gym.ReviewCount,
                IsOpen = gym.IsOpenNow(),
                IsFavorite = isFavorite,
                FavoriteCount = gym.FavoritedByUsers.Count,

                Amenities = gym.Amenities
                    .OrderBy(a => a.DisplayOrder)
                    .Select(a => new GymAmenityDto
                    {
                        Id = a.Id,
                        Type = a.AmenityType.ToString(),
                        Name = a.GetDisplayName(),
                        IconName = a.IconName,
                        IsHighlighted = a.IsHighlighted
                    }).ToList(),

                OpeningHours = gym.OpeningHours
                    .OrderBy(o => (int)o.DayOfWeek == 0 ? 7 : (int)o.DayOfWeek)
                    .Select(o => new OpeningHourResponseDto
                    {
                        Day = o.DayOfWeek.ToString(),
                        OpenTime = o.OpenTime.ToString(@"hh\:mm"),
                        CloseTime = o.ClosingTime.ToString(@"hh\:mm"),
                        IsClosed = o.IsClosed,
                        DisplayText = o.IsClosed ? "Closed" : $"{o.OpenTime:hh\\:mm} - {o.ClosingTime:hh\\:mm}"
                    }).ToList(),

                Packages = gym.Packages
                    .OrderBy(p => p.Price)
                    .Select(p => new PackageDetailDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        Price = p.Price,
                        DurationInDays = p.DurationInDays,
                        IsPopular = p.IsPopular,
                        Features = p.Features
                            .OrderBy(f => f.DisplayOrder)
                            .Select(f => f.Name)
                            .ToList(),
                        DurationLabel = $"{p.DurationInDays} days",
                        PriceLabel = $"₼{p.Price:0}"
                    }).ToList(),

                Trainers = gym.Trainers
                    .Select(t => new TrainerListDto
                    {
                        Id = t.Id,
                        UserFullName = t.User.FullName,
                        Specialization = t.Specialization,
                        ProfileImageUrl = t.ProfileImageUrl,
                        SessionPrice = t.SessionPrice,
                        AverageRating = t.AverageRating,
                        ReviewCount = t.ReviewCount,
                        GymName = gym.Name,
                        IsOnlineTrainer = t.IsOnlineTrainer,
                        IsVerified = t.IsVerified
                    }).ToList(),

                Phone = gym.Phone,
                Email = gym.Email,
                Website = gym.Website,
                Latitude = gym.Latitude,
                Longitude = gym.Longitude
            };

            return ResponseModel<GymDetailDto>.SuccessResponse(dto);
        }
    }
