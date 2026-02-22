using Startup_Project.Application.Dtos.Payment;
using Startup_Project.Application.Models;

namespace Startup_Project.Application.Services.Interfaces;

public interface ISessionPaymentService
{
    Task<ResponseModel<SessionPaymentResponseDto>> CreatePaymentAsync(SessionPaymentRequestDto request);
    Task<ResponseModel<bool>> HandleCallbackAsync(string sessionId);
    Task<ResponseModel<SessionPaymentStatusDto>> GetPaymentStatusAsync(string orderId);
    Task<ResponseModel<MyPaymentsResponseDto>> GetMyPaymentsAsync();
}
