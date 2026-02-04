using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Startup_Project.Application.Dtos.Payment;
using Startup_Project.Application.Services.Interfaces;
using Startup_Project.Domain.Exceptions;
using System.Security.Claims;

namespace Startup_Project.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentController(IPaymentService service, IConfiguration configuration) : ControllerBase
{
    [HttpPost("initiate")]
    [Authorize]
    public async Task<IActionResult> InitiatePayment([FromBody] PaymentRequestDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            throw BusinessException.UnauthorizedAccess();

        return Ok(await service.CreatePaymentSessionAsync(userId, dto));
    }

    [HttpPost("result")]
    [AllowAnonymous]
    public async Task<IActionResult> PaymentResult([FromQuery] string sessionId)
    {
        if (string.IsNullOrEmpty(sessionId))
            throw BusinessException.ValidationFailed("SessionId boş ola bilməz.");

        var successUrl = configuration["PaymentSettings:SuccessUrl"] ?? "/payment/success";
        var failUrl = configuration["PaymentSettings:FailUrl"] ?? "/payment/fail";

        var result = await service.FinalizePaymentAsync(sessionId);
        return Redirect(result.Success ? successUrl : failUrl);
    }
}
