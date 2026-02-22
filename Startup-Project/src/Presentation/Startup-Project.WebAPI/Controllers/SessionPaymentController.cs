using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Startup_Project.Application.Dtos.Payment;
using Startup_Project.Application.Services.Interfaces;
using Startup_Project.Domain.Exceptions;

namespace Startup_Project.WebAPI.Controllers;

[Route("api/session-payment")]
[ApiController]
public class SessionPaymentController(
    ISessionPaymentService service,
    IConfiguration configuration) : ControllerBase
{
    [HttpPost("initiate")]
    [Authorize]
    public async Task<IActionResult> InitiatePayment([FromBody] SessionPaymentRequestDto request)
        => Ok(await service.CreatePaymentAsync(request));

    [HttpPost("callback")]
    [AllowAnonymous]
    public async Task<IActionResult> PaymentCallback([FromQuery] string sessionId)
    {
        if (string.IsNullOrEmpty(sessionId))
            throw BusinessException.ValidationFailed("SessionId bos ola bilmez.");

        var successUrl = configuration["PaymentSettings:SuccessUrl"] ?? "/payment/success";
        var failUrl = configuration["PaymentSettings:FailUrl"] ?? "/payment/fail";

        try
        {
            await service.HandleCallbackAsync(sessionId);
            return Redirect(successUrl);
        }
        catch
        {
            return Redirect(failUrl);
        }
    }

    [HttpGet("status/{orderId}")]
    [Authorize]
    public async Task<IActionResult> GetPaymentStatus(string orderId)
        => Ok(await service.GetPaymentStatusAsync(orderId));

    [HttpGet("my-payments")]
    [Authorize]
    public async Task<IActionResult> GetMyPayments()
        => Ok(await service.GetMyPaymentsAsync());
}
