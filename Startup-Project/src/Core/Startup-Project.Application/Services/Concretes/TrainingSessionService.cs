using Startup_Project.Application.Dtos.TrainingSession;
using Startup_Project.Application.Models;
using Startup_Project.Application.Services.Interfaces;

namespace Startup_Project.Application.Services.Concretes;

public class TrainingSessionService : ITrainingSessionService
{
    public Task<ResponseModel<bool>> CancelSessionAsync(Guid sessionId, SessionCancelDto dto)
    {
        throw new NotImplementedException();
    }

    public Task<ResponseModel<bool>> ConfirmPaymentAsync(Guid sessionId, Guid paymentId)
    {
        throw new NotImplementedException();
    }

    public Task<ResponseModel<List<SessionDto>>> GetGymSessionsAsync()
    {
        throw new NotImplementedException();
    }

    public Task<ResponseModel<List<SessionDto>>> GetMySessionsAsync()
    {
        throw new NotImplementedException();
    }

    public Task<ResponseModel<SessionDto>> GetSessionByIdAsync(Guid sessionId)
    {
        throw new NotImplementedException();
    }

    public Task<ResponseModel<SessionQrDto>> GetSessionQrAsync(Guid sessionId)
    {
        throw new NotImplementedException();
    }

    public Task<ResponseModel<List<SessionDto>>> GetTrainerSessionsAsync()
    {
        throw new NotImplementedException();
    }

    public Task<ResponseModel<string>> PurchaseSessionAsync(SessionPurchaseDto dto)
    {
        throw new NotImplementedException();
    }

    public Task<ResponseModel<SessionVerifyResultDto>> VerifyAndUseSessionAsync(SessionVerifyDto dto)
    {
        throw new NotImplementedException();
    }
}
