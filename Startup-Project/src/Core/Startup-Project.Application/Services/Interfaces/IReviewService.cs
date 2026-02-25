using Startup_Project.Application.Dtos.GymDtos.GymReview;
using Startup_Project.Application.Models;

namespace Startup_Project.Application.Services.Interfaces;

public interface IReviewService
{
    Task<ResponseModel<bool>> AddReviewAsync(ReviewCreateDto dto);
    Task<ResponseModel<PaginatedReviewsDto>> GetGymReviewsAsync(Guid gymId, int page = 1, int pageSize = 10);
    Task<ResponseModel<bool>> DeleteReviewAsync(string reviewId);
}
