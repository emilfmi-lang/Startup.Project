using Startup_Project.Application.Dtos.Payment;
using Startup_Project.Application.Models;

namespace Startup_Project.Application.Services.Interfaces;

public interface IPaymentService
{
    Task<ResponseModel<PaymentResponseDto>> CreatePaymentSessionAsync(Guid userId, PaymentRequestDto dto);
    Task<ResponseModel<bool>> FinalizePaymentAsync(string sessionId);

}
