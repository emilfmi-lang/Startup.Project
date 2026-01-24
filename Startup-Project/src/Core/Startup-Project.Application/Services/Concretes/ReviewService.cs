

using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Startup_Project.Application.Dtos.GymDtos.GymReview;
using Startup_Project.Application.Models;
using Startup_Project.Application.Services.Interfaces;
using Startup_Project.Domain.Abstractions.Repositories;
using Startup_Project.Domain.Entities;
using System.Security.Claims;

namespace Startup_Project.Application.Services.Concretes;

public class ReviewService(
    IGenericRepository<GymReview> reviewRepository,
    IGenericRepository<Gym> repository ,
    IMapper mapper,
    IHttpContextAccessor httpContextAccessor
    ): IReviewService
{
    public async Task<ResponseModel<bool>> AddReviewAsync(ReviewCreateDto dto)
    {
        var userId = httpContextAccessor.HttpContext?
            .User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return ResponseModel<bool>.FailureResponse("Rəy bildirmək üçün qeydiyyatdan keçməlisiniz.");

        if (!Guid.TryParse(userId, out var userGuid))
            return ResponseModel<bool>.FailureResponse("İstifadəçi məlumatı yanlışdır.");

        var validator = new ReviewCreateDtoValidator();
        var validationResult = await validator.ValidateAsync(dto);

        if (!validationResult.IsValid)
            return ResponseModel<bool>.FailureResponse(
                validationResult.Errors.Select(e => e.ErrorMessage).ToList()
            );

        var gym = await repository.GetByIdAsync(dto.GymId);

        if (gym == null)
            return ResponseModel<bool>.FailureResponse("Rəy yazmaq istədiyiniz zal tapılmadı.");

        var hasAlreadyReviewed = await reviewRepository.AnyAsync(
            r => r.GymId == dto.GymId && r.UserId == userGuid && !r.IsDeleted
        );

        if (hasAlreadyReviewed)
            return ResponseModel<bool>.FailureResponse("Siz bu zala artıq qiymət vermisiniz.");

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
                return ResponseModel<bool>.FailureResponse("Rəy ID-si yanlışdır.");

            var review = await reviewRepository.GetByIdAsync(reviewId, tracking: true);

            if (review == null || review.IsDeleted)
                return ResponseModel<bool>.FailureResponse("Rəy tapılmadı.");

            var userId = httpContextAccessor.HttpContext?
                .User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return ResponseModel<bool>.FailureResponse("İstifadəçi təyin edilməyib.");

            if (!Guid.TryParse(userId, out var userGuid))
                return ResponseModel<bool>.FailureResponse("İstifadəçi məlumatı yanlışdır.");

            if (review.UserId != userGuid)
                return ResponseModel<bool>.FailureResponse("Bu rəyi yalnız özünüz silə bilərsiniz.");

            var gym = await repository.GetByIdAsync(review.GymId.ToString(), tracking: true);
            if (gym == null)
                return ResponseModel<bool>.FailureResponse("Zal tapılmadı.");

            review.IsDeleted = true;
            reviewRepository.Update(review);

            gym.RemoveReview(review.StarCount);
            repository.Update(gym);

            await reviewRepository.SaveAsync();
            await repository.SaveAsync();

            return ResponseModel<bool>.SuccessResponse(true);
        }

    }
