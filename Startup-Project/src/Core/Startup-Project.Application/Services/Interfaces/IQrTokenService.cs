using Startup_Project.Application.Dtos.QrCode;
using Startup_Project.Application.Models;

namespace Startup_Project.Application.Services.Interfaces;

public interface IQrTokenService
{
    Task<ResponseModel<QrCodeResponseDto>> GenerateQrTokenAsync(Guid userId, Guid gymId);
    Task<ResponseModel<List<QrCodeResponseDto>>> GetMyActiveQrCodesAsync(Guid userId);
    Task<ResponseModel<QrVerifyResponseDto>> VerifyQrTokenAsync(QrVerifyRequestDto request);
    Task<ResponseModel<bool>> CheckOutAsync(Guid checkInLogId);
    Task<ResponseModel<List<CheckInHistoryItemDto>>> GetMyCheckInHistoryAsync(Guid userId, int page = 1, int pageSize = 20);
    Task<ResponseModel<List<CheckInHistoryItemDto>>> GetGymCheckInsAsync(Guid gymId, DateTime? date = null, int page = 1, int pageSize = 50);
        Task<ResponseModel<GymCheckInStatsDto>> GetGymCheckInStatsAsync(Guid gymId);
    }
