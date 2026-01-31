using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Startup_Project.Application.Dtos.Payment;
using Startup_Project.Application.Services.Interfaces;
using Startup_Project.Domain.Exceptions;
using System.Security.Claims;

namespace Startup_Project.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentController(IPaymentService service) : ControllerBase
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
        var result = await service.FinalizePaymentAsync(sessionId);
        if (result.Success)
            return Redirect("https://senin-app-saytin.com/payment/success");
        return Redirect("https://senin-app-saytin.com/payment/fail");
    }
}
