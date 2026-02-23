using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Startup_Project.Application.Dtos.PromoCode;
using Startup_Project.Application.Services.Interfaces;

namespace Startup_Project.WebAPI.Controllers;

[Route("api/promo-codes")]
[ApiController]
[Authorize]
public class PromoCodesController(IPromoCodeService promoCodeService) : ControllerBase
{
    [HttpGet("validate")]
    public async Task<IActionResult> Validate([FromQuery] string code, [FromQuery] Guid packageId)
        => Ok(await promoCodeService.ValidateAsync(code, packageId));

    [HttpPost("apply")]
    public async Task<IActionResult> Apply([FromBody] ApplyPromoCodeDto dto)
        => Ok(await promoCodeService.ApplyAsync(dto));

    [HttpGet("validate-session")]
    public async Task<IActionResult> ValidateForSession([FromQuery] string code, [FromQuery] Guid scheduledSessionId)
        => Ok(await promoCodeService.ValidateForSessionAsync(code, scheduledSessionId));

    [HttpPost("apply-session")]
    public async Task<IActionResult> ApplyForSession([FromBody] ApplySessionPromoCodeDto dto)
        => Ok(await promoCodeService.ApplyForSessionAsync(dto));
}
