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

        var hasAlreadyReviewed = await reviewRepository.AnyAsync(
            r => r.GymId == dto.GymId && r.UserId == userGuid && !r.IsDeleted
        );
        if (hasAlreadyReviewed)
            throw BusinessException.AlreadyReviewed();

        var review = mapper.Map<GymReview>(dto);
        review.UserId = userGuid;

        await reviewRepository.AddAsync(review);
        gym.AddReview(review.StarCount);

        await reviewRepository.SaveAsync();
        await repository.SaveAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }

    public async Task<ResponseModel<List<ReviewGetDto>>> GetGymReviewsAsync(Guid gymId)
    {
        var reviews = await reviewRepository.Table
            .Include(r => r.User)
            .Where(r => r.GymId == gymId && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .AsNoTracking()
            .ToListAsync();

        var dtos = mapper.Map<List<ReviewGetDto>>(reviews);
        return ResponseModel<List<ReviewGetDto>>.SuccessResponse(dtos);
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
