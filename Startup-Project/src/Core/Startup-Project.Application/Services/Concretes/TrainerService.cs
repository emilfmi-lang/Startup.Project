using Startup_Project.Application.Dtos.Trainer;
using Startup_Project.Application.Dtos.TrainerReview;
using Startup_Project.Application.Dtos.TrainerSchedule;
using Startup_Project.Application.Models;
using Startup_Project.Application.Services.Interfaces;

namespace Startup_Project.Application.Services.Concretes;

public class TrainerService : ITrainerService
{
    public Task<ResponseModel<bool>> ActivateTrainerAsync(Guid trainerId)
    {
        throw new NotImplementedException();
    }

    public Task<ResponseModel<bool>> AddReviewAsync(Guid trainerId, TrainerReviewCreateDto dto)
    {
        throw new NotImplementedException();
    }

    public Task<ResponseModel<bool>> CreateTrainerAsync(TrainerCreateDto dto)
    {
        throw new NotImplementedException();
    }

    public Task<ResponseModel<bool>> DeactivateTrainerAsync(Guid trainerId)
    {
        throw new NotImplementedException();
    }

    public Task<ResponseModel<bool>> DeleteTrainerAsync(Guid trainerId)
    {
        throw new NotImplementedException();
    }

    public Task<ResponseModel<List<TrainerListDto>>> GetAllTrainersAsync()
    {
        throw new NotImplementedException();
    }

    public Task<ResponseModel<TrainerDto>> GetMyProfileAsync()
    {
        throw new NotImplementedException();
    }

    public Task<ResponseModel<TrainerDto>> GetTrainerByIdAsync(Guid trainerId)
    {
        throw new NotImplementedException();
    }

    public Task<ResponseModel<List<TrainerReviewDto>>> GetTrainerReviewsAsync(Guid trainerId)
    {
        throw new NotImplementedException();
    }

    public Task<ResponseModel<List<TrainerListDto>>> GetTrainersByGymAsync(Guid gymId)
    {
        throw new NotImplementedException();
    }

    public Task<ResponseModel<List<TrainerScheduleDto>>> GetTrainerScheduleAsync(Guid trainerId)
    {
        throw new NotImplementedException();
    }

    public Task<ResponseModel<bool>> SetTrainerScheduleAsync(Guid trainerId, List<TrainerScheduleDto> schedules)
    {
        throw new NotImplementedException();
    }

    public Task<ResponseModel<bool>> UpdateMyProfileAsync(TrainerUpdateDto dto)
    {
        throw new NotImplementedException();
    }

    public Task<ResponseModel<bool>> UpdateMyScheduleAsync(List<TrainerScheduleDto> schedules)
    {
        throw new NotImplementedException();
    }

    public Task<ResponseModel<bool>> UpdateTrainerAsync(Guid trainerId, TrainerUpdateDto dto)
    {
        throw new NotImplementedException();
    }
}
