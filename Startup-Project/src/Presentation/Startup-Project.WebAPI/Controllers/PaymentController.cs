using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Startup_Project.Application.Dtos.Payment;
using Startup_Project.Application.Services.Interfaces;

namespace Startup_Project.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentController(IPaymentService service) : ControllerBase
{
    [HttpPost("initiate")]
    public async Task<IActionResult> InitiatePayment([FromBody] PaymentRequestDto dto)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return Unauthorized("İstifadəçi tapılmadı.");
        var userId = Guid.Parse(userIdClaim.Value);
        var result = await service.CreatePaymentSessionAsync(userId, dto);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }
    [HttpPost("result")]
    [AllowAnonymous]
    public async Task<IActionResult> PaymentResult([FromQuery] string sessionId)
    {
        if(string.IsNullOrEmpty(sessionId))
            return BadRequest("SessionId boş ola bilməz.");

        var result = await service.FinalizePaymentAsync(sessionId);
        if(result.Success)
            return Redirect("https://senin-app-saytin.com/payment/success");
        return Redirect("https://senin-app-saytin.com/payment/fail");
    }
}
