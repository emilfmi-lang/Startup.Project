using Startup_Project.Application.Dtos.Payment;
using Startup_Project.Application.Models;

namespace Startup_Project.Application.Services.Interfaces;

public interface IPaymentService
{
    Task<ResponseModel<PaymentResponseDto>> CreatePackagePaymentAsync(PaymentRequestDto request);
    Task<ResponseModel<bool>> HandlePackageCallbackAsync(string sessionId);
}
