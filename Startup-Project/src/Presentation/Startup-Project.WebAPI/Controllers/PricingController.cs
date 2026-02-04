using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Startup_Project.Application.Services.Interfaces;

namespace Startup_Project.WebAPI.Controllers;

[Route("api/pricing")]
[ApiController]
public class PricingController(IPricingEngine pricingEngine) : ControllerBase
{
    [HttpPost("calculate")]
    [Authorize]
    public async Task<IActionResult> Calculate([FromBody] PricingCalculateRequest request)
        => Ok(await pricingEngine.CalculateAsync(request.PackageId, request.PromoCode));

    [HttpGet("preview/{packageId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> Preview(Guid packageId)
        => Ok(await pricingEngine.PreviewAsync(packageId));
}

public record PricingCalculateRequest(Guid PackageId, string? PromoCode);
