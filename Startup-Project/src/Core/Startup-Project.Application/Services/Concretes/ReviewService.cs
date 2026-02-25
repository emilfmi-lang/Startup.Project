using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Startup_Project.Application.Dtos.GymDtos.GymReview;
using Startup_Project.Application.Models;
using Startup_Project.Application.Services.Interfaces;
using Startup_Project.Domain.Abstractions.Repositories;
using Startup_Project.Domain.Entities;
using Startup_Project.Domain.Exceptions;
using System.Security.Claims;

namespace Startup_Project.Application.Services.Concretes;

public class ReviewService(
    IGenericRepository<GymReview> reviewRepository,
    IGenericRepository<Gym> repository,
    IMapper mapper,
    IHttpContextAccessor httpContextAccessor
    ) : IReviewService
{
    private Guid GetCurrentUserId()
    {
        var userId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            throw BusinessException.UnauthorizedAccess();
        return userGuid;
    }

    public async Task<ResponseModel<bool>> AddReviewAsync(ReviewCreateDto dto)
    {
        var userGuid = GetCurrentUserId();

        var validator = new ReviewCreateDtoValidator();
        var validationResult = await validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            throw BusinessException.ValidationFailed(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

        var gym = await repository.GetByIdAsync(dto.GymId);
        if (gym == null)
            throw NotFoundException.Gym(dto.GymId);

        var existingReview = await reviewRepository.Table
            .FirstOrDefaultAsync(r => r.GymId == dto.GymId && r.UserId == userGuid);

        if (existingReview != null)
        {
            if (!existingReview.IsDeleted)
                throw BusinessException.AlreadyReviewed();

            existingReview.Reactivate(dto.StarCount, dto.Comment);
            reviewRepository.Update(existingReview);
            gym.AddReview(dto.StarCount);
        }
        else
        {
            var review = mapper.Map<GymReview>(dto);
            review.UserId = userGuid;
            await reviewRepository.AddAsync(review);
            gym.AddReview(review.StarCount);
        }

        await reviewRepository.SaveAsync();
        await repository.SaveAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }

    public async Task<ResponseModel<PaginatedReviewsDto>> GetGymReviewsAsync(Guid gymId, int page = 1, int pageSize = 10)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var query = reviewRepository.Table
            .Include(r => r.User)
            .Where(r => r.GymId == gymId && !r.IsDeleted);

        var totalCount = await query.CountAsync();

        var averageRating = totalCount > 0
            ? await query.AverageAsync(r => (double)r.StarCount)
            : 0;

        var reviews = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();

        var dtos = reviews.Select(r => new ReviewGetDto
        {
            Id = r.Id,
            Comment = r.Comment,
            StarCount = r.StarCount,
            UserFullName = r.User?.FullName ?? "İstifadəçi",
            UserEmail = r.User?.Email,
            UserProfileImageUrl = r.User?.ProfileImageUrl,
            CreatedAt = r.CreatedAt
        }).ToList();

        var result = new PaginatedReviewsDto
        {
            Reviews = dtos,
            TotalCount = totalCount,
            AverageRating = Math.Round(averageRating, 1),
            Page = page,
            PageSize = pageSize
        };

        return ResponseModel<PaginatedReviewsDto>.SuccessResponse(result);
    }

    public async Task<ResponseModel<bool>> DeleteReviewAsync(string reviewId)
    {
        if (!Guid.TryParse(reviewId, out var reviewGuid))
            throw BusinessException.ValidationFailed("Rəy ID-si yanlışdır.");

        var review = await reviewRepository.GetByIdAsync(reviewId, tracking: true);
        if (review == null || review.IsDeleted)
            throw NotFoundException.Review(reviewGuid);

        var userGuid = GetCurrentUserId();
        if (review.UserId != userGuid)
            throw BusinessException.UnauthorizedAccess();

        var gym = await repository.GetByIdAsync(review.GymId.ToString(), tracking: true);
        if (gym == null)
            throw NotFoundException.Gym(review.GymId);

        review.IsDeleted = true;
        reviewRepository.Update(review);

        gym.RemoveReview(review.StarCount);
        repository.Update(gym);

        await reviewRepository.SaveAsync();
        await repository.SaveAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }
}
