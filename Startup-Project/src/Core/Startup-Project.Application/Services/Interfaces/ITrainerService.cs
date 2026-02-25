using Startup_Project.Application.Dtos.Trainer;
using Startup_Project.Application.Dtos.TrainerReview;
using Startup_Project.Application.Dtos.TrainerSchedule;
using Startup_Project.Application.Models;

namespace Startup_Project.Application.Services.Interfaces;

public interface ITrainerService
{
    // Public endpoints
    Task<ResponseModel<List<TrainerListDto>>> GetAllTrainersAsync();
    Task<ResponseModel<TrainerDetailDto>> GetTrainerDetailAsync(Guid trainerId);
    Task<ResponseModel<List<TrainerListDto>>> GetTrainersByGymAsync(Guid gymId);
    Task<ResponseModel<PaginatedTrainerReviewsDto>> GetTrainerReviewsAsync(Guid trainerId, int page = 1, int pageSize = 10);
    Task<ResponseModel<List<TrainerScheduleDto>>> GetTrainerScheduleAsync(Guid trainerId);
    Task<ResponseModel<bool>> AddReviewAsync(Guid trainerId, TrainerReviewCreateDto dto);
    Task<ResponseModel<bool>> DeleteTrainerReviewAsync(Guid trainerId, string reviewId);

    // Online Trainer endpoints
    Task<ResponseModel<List<TrainerListDto>>> GetOnlineTrainersAsync();
    Task<ResponseModel<bool>> ApplyAsOnlineTrainerAsync(OnlineTrainerApplyDto dto);

    // GymOwner endpoints
    Task<ResponseModel<bool>> CreateTrainerAsync(TrainerCreateDto dto);
    Task<ResponseModel<bool>> UpdateTrainerAsync(Guid trainerId, TrainerUpdateDto dto);
    Task<ResponseModel<bool>> DeleteTrainerAsync(Guid trainerId);
    Task<ResponseModel<bool>> ActivateTrainerAsync(Guid trainerId);
    Task<ResponseModel<bool>> DeactivateTrainerAsync(Guid trainerId);
    Task<ResponseModel<bool>> SetTrainerScheduleAsync(Guid trainerId, List<TrainerScheduleDto> schedules);

    // Admin endpoints
    Task<ResponseModel<List<TrainerListDto>>> GetPendingOnlineTrainersAsync();
    Task<ResponseModel<bool>> VerifyOnlineTrainerAsync(Guid trainerId, TrainerVerifyDto dto);

    // Trainer own profile
    Task<ResponseModel<TrainerDto>> GetMyProfileAsync();
    Task<ResponseModel<bool>> UpdateMyProfileAsync(TrainerUpdateDto dto);
    Task<ResponseModel<bool>> UpdateMyScheduleAsync(List<TrainerScheduleDto> schedules);
}
