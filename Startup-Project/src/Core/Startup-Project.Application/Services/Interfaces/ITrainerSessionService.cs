using Startup_Project.Application.Dtos.Trainer;
using Startup_Project.Application.Models;

namespace Startup_Project.Application.Services.Interfaces;

/// <summary>
/// Trainer paneli — seans idareetmesi
/// TrainerId JWT-den avtomatik goturulur
/// </summary>
public interface ITrainerSessionService
{
    /// <summary>
    /// Trainerin oz seanslarini getirir (JWT-den trainerId)
    /// </summary>
    Task<ResponseModel<List<TrainerSessionResponseDto>>> GetMySessionsAsync();

    /// <summary>
    /// Yeni seans yaradir (JWT-den trainerId, body-den seans melumatlari)
    /// </summary>
    Task<ResponseModel<TrainerSessionResponseDto>> CreateSessionAsync(TrainerCreateSessionDto request);
}
