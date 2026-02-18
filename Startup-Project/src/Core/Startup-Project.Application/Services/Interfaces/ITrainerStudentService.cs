using Startup_Project.Application.Dtos.Trainer;
using Startup_Project.Application.Models;

namespace Startup_Project.Application.Services.Interfaces;

public interface ITrainerStudentService
{
    Task<ResponseModel<TrainerStudentsPageDto>> GetMyStudentsAsync(
        string? search = null,
        string? status = null);

    Task<ResponseModel<TrainerStudentDto>> AddStudentAsync(AddStudentDto request);

    Task<ResponseModel<bool>> RemoveStudentAsync(Guid studentId);
}
