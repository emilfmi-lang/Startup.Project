

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
        var userId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if(string.IsNullOrEmpty(userId))
        {
            return ResponseModel<bool>.FailureResponse("Rəy bildirmək üçün qeydiyyatdan keçməlisiniz.");

        }
        var userGuid = Guid.Parse(userId);
        var validator = new ReviewCreateDtoValidator();
        var validationResult = await validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return ResponseModel<bool>.FailureResponse(validationResult.Errors.Select(e => e.ErrorMessage).ToList());
        }
        var gym = await repository.GetByIdAsync(dto.GymId.ToString());
        if (gym == null)
        {
            return ResponseModel<bool>.FailureResponse("Rəy yazmaq istədiyiniz zal tapılmadı.");
        }
        var hasAlreadyReviewed = await reviewRepository.AnyAsync(r => r.GymId == dto.GymId && r.UserId == userGuid);
        if (hasAlreadyReviewed)
        {
            return ResponseModel<bool>.FailureResponse("Siz bu zala artıq qiymət vermisiniz.");
        }
        var review = mapper.Map<GymReview>(dto);
        var result = await reviewRepository.AddAsync(review);
        if (!result) return ResponseModel<bool>.FailureResponse("Xəta baş verdi, rəy qeydə alınmadı.");
        await reviewRepository.SaveAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }
    public async Task<ResponseModel<List<ReviewGetDto>>> GetGymReviewsAsync(Guid gymId)
    {
        var reviews = await reviewRepository.Table
            .Include(r => r.User)
            .Where(r => r.GymId == gymId)
            .OrderByDescending(r => r.CreatedAt) 
            .AsNoTracking()
            .ToListAsync();
        var dtos = mapper.Map<List<ReviewGetDto>>(reviews);
        return ResponseModel<List<ReviewGetDto>>.SuccessResponse(dtos);
    }
}
