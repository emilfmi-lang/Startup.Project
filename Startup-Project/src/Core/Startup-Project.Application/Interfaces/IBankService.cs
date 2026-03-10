using Startup_Project.Application.Dtos.Payment;

namespace Startup_Project.Application.Interfaces;

public interface IBankService
{
    Task<PaymentResponseDto> CreateSessionAsync(decimal amount, string packageName, string callbackPath = "/api/payment/package-callback");
    Task<bool> CheckPaymentStatusAsync(string sessionId);
}
