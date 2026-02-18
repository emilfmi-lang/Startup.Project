using Startup_Project.Application.Dtos.Trainer;
using Startup_Project.Application.Models;

namespace Startup_Project.Application.Services.Interfaces;

public interface ITrainerSessionService
{
    Task<ResponseModel<List<TrainerSessionResponseDto>>> GetMySessionsAsync(
        string? search = null,
        string? status = null);

    Task<ResponseModel<TrainerSessionResponseDto>> CreateSessionAsync(TrainerCreateSessionDto request);

    Task<ResponseModel<TrainerSessionResponseDto>> UpdateSessionStatusAsync(
        Guid sessionId,
        string status);
}
