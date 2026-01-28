

using Startup_Project.Application.Dtos.TrainingSession;
using Startup_Project.Application.Models;

namespace Startup_Project.Application.Services.Interfaces;

public interface ITrainingSessionService
{
    Task<ResponseModel<string>> PurchaseSessionAsync(SessionPurchaseDto dto);
    Task<ResponseModel<List<SessionDto>>> GetMySessionsAsync();
    Task<ResponseModel<SessionDto>> GetSessionByIdAsync(Guid sessionId);
    Task<ResponseModel<SessionQrDto>> GetSessionQrAsync(Guid sessionId);
    Task<ResponseModel<bool>> CancelSessionAsync(Guid sessionId, SessionCancelDto dto);
    Task<ResponseModel<SessionVerifyResultDto>> VerifyAndUseSessionAsync(SessionVerifyDto dto);
    Task<ResponseModel<List<SessionDto>>> GetTrainerSessionsAsync();
    Task<ResponseModel<List<SessionDto>>> GetGymSessionsAsync();
    Task<ResponseModel<bool>> ConfirmPaymentAsync(Guid sessionId, Guid paymentId);

}
