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
    IConfiguration configuration,
    ILogger<SessionPaymentController> logger) : ControllerBase
{
    [HttpPost("initiate")]
    [Authorize]
    public async Task<IActionResult> InitiatePayment([FromBody] SessionPaymentRequestDto request)
        => Ok(await service.CreatePaymentAsync(request));

    [HttpGet("callback")]
    [HttpPost("callback")]
    [AllowAnonymous]
    public async Task<IActionResult> PaymentCallback([FromQuery] string sessionId)
    {
        if (string.IsNullOrEmpty(sessionId))
            throw BusinessException.ValidationFailed("SessionId bos ola bilmez.");

        var successUrl = GetRedirectUrl("PaymentSettings:SuccessUrl", "/payment/success");
        var failUrl = GetRedirectUrl("PaymentSettings:FailUrl", "/payment/fail");

        try
        {
            await service.HandleCallbackAsync(sessionId);
            logger.LogInformation("Session payment callback SUCCESS — SessionId={SessionId}, RedirectTo={Url}",
                sessionId, successUrl);
            return Redirect(successUrl);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Session payment callback FAILED — SessionId={SessionId}, Error={Error}",
                sessionId, ex.Message);
            return Redirect(failUrl);
        }
    }

    private string GetRedirectUrl(string configKey, string fallback)
    {
        var url = configuration[configKey];

        if (string.IsNullOrEmpty(url))
            return fallback;

        if (url.Contains("senin-app-saytin") || url.Contains("example.com"))
        {
            logger.LogWarning("Config '{Key}' has placeholder URL: {Url}, using fallback", configKey, url);
            return fallback;
        }

        return url;
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
