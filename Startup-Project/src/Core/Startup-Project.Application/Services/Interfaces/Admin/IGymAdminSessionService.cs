using Startup_Project.Application.Dtos.Admin;
using Startup_Project.Application.Models;

namespace Startup_Project.Application.Services.Interfaces.Admin;

public interface IGymAdminSessionService
{
    Task<ResponseModel<List<SessionResponseDto>>> GetTrainerSessionsAsync(Guid trainerId);
    Task<ResponseModel<SessionResponseDto>> CreateSessionAsync(CreateSessionDto request);
    Task<ResponseModel<SessionResponseDto>> UpdateSessionStatusAsync(Guid sessionId, UpdateSessionStatusDto request);
}
