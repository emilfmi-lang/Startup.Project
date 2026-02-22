using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Startup_Project.Application.Dtos.Payment;
using Startup_Project.Application.Services.Interfaces;
using Startup_Project.Domain.Exceptions;

namespace Startup_Project.WebAPI.Controllers;

[Route("api/payment")]
[ApiController]
public class PaymentController(IPaymentService service, IConfiguration configuration) : ControllerBase
{
    [HttpPost("initiate-package")]
    [Authorize]
    public async Task<IActionResult> InitiatePackagePayment([FromBody] PaymentRequestDto request)
        => Ok(await service.CreatePackagePaymentAsync(request));

    [HttpPost("package-callback")]
    [AllowAnonymous]
    public async Task<IActionResult> PackageCallback([FromQuery] string sessionId)
    {
        if (string.IsNullOrEmpty(sessionId))
            throw BusinessException.ValidationFailed("SessionId bos ola bilmez.");

        var successUrl = configuration["PaymentSettings:SuccessUrl"] ?? "/payment/success";
        var failUrl = configuration["PaymentSettings:FailUrl"] ?? "/payment/fail";

        try
        {
            await service.HandlePackageCallbackAsync(sessionId);
            return Redirect(successUrl);
        }
        catch
        {
            return Redirect(failUrl);
        }
    }
}
