using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Startup_Project.Application.Dtos.Payment;
using Startup_Project.Application.Services.Interfaces;
using Startup_Project.Domain.Exceptions;

namespace Startup_Project.WebAPI.Controllers;

[Route("api/payment")]
[ApiController]
public class PaymentController(
    IPaymentService service,
    IConfiguration configuration,
    ILogger<PaymentController> logger) : ControllerBase
{
    [HttpPost("initiate-package")]
    [Authorize]
    public async Task<IActionResult> InitiatePackagePayment([FromBody] PaymentRequestDto request)
        => Ok(await service.CreatePackagePaymentAsync(request));

    [HttpGet("package-callback")]
    [HttpPost("package-callback")]
    [AllowAnonymous]
    public async Task<IActionResult> PackageCallback([FromQuery] string sessionId)
    {
        if (string.IsNullOrEmpty(sessionId))
            throw BusinessException.ValidationFailed("SessionId bos ola bilmez.");

        var successUrl = GetRedirectUrl("PaymentSettings:SuccessUrl", "/payment/success");
        var failUrl = GetRedirectUrl("PaymentSettings:FailUrl", "/payment/fail");

        try
        {
            await service.HandlePackageCallbackAsync(sessionId);
            logger.LogInformation("Payment callback SUCCESS — SessionId={SessionId}, RedirectTo={Url}",
                sessionId, successUrl);
            return Redirect(successUrl);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Payment callback FAILED — SessionId={SessionId}, Error={Error}",
                sessionId, ex.Message);
            return Redirect(failUrl);
        }
    }

    private string GetRedirectUrl(string configKey, string fallback)
    {
        var url = configuration[configKey];

        if (string.IsNullOrEmpty(url))
        {
            logger.LogWarning("Config key '{Key}' is empty, using fallback: {Fallback}", configKey, fallback);
            return fallback;
        }

        // Production URL-lərini localhost-da istifadə etmə
        if (url.Contains("senin-app-saytin") || url.Contains("example.com"))
        {
            logger.LogWarning("Config key '{Key}' has placeholder URL: {Url}, using fallback: {Fallback}",
                configKey, url, fallback);
            return fallback;
        }

        return url;
    }
}
