using Startup_Project.Application.Dtos.Trainer;
using Startup_Project.Application.Dtos.TrainerReview;
using Startup_Project.Application.Dtos.TrainerSchedule;
using Startup_Project.Application.Models;

namespace Startup_Project.Application.Services.Interfaces;

public interface ITrainerService
{
    Task<ResponseModel<List<TrainerListDto>>> GetAllTrainersAsync();
    Task<ResponseModel<TrainerDto>> GetTrainerByIdAsync(Guid trainerId);
    Task<ResponseModel<List<TrainerListDto>>> GetTrainersByGymAsync(Guid gymId);
    Task<ResponseModel<List<TrainerReviewDto>>> GetTrainerReviewsAsync(Guid trainerId);
    Task<ResponseModel<List<TrainerScheduleDto>>> GetTrainerScheduleAsync(Guid trainerId);
    Task<ResponseModel<bool>> AddReviewAsync(Guid trainerId, TrainerReviewCreateDto dto);

    // GymOwner endpoints
    Task<ResponseModel<bool>> CreateTrainerAsync(TrainerCreateDto dto);
    Task<ResponseModel<bool>> UpdateTrainerAsync(Guid trainerId, TrainerUpdateDto dto);
    Task<ResponseModel<bool>> DeleteTrainerAsync(Guid trainerId);
    Task<ResponseModel<bool>> ActivateTrainerAsync(Guid trainerId);
    Task<ResponseModel<bool>> DeactivateTrainerAsync(Guid trainerId);
    Task<ResponseModel<bool>> SetTrainerScheduleAsync(Guid trainerId, List<TrainerScheduleDto> schedules);
    Task<ResponseModel<TrainerDto>> GetMyProfileAsync();
    Task<ResponseModel<bool>> UpdateMyProfileAsync(TrainerUpdateDto dto);
    Task<ResponseModel<bool>> UpdateMyScheduleAsync(List<TrainerScheduleDto> schedules);
}
