using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Startup_Project.Application.Services.Interfaces;

namespace Startup_Project.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class QrTokenController(IQrTokenService service) : ControllerBase
{
    [Authorize]
    [HttpPost("generate")]
    public async Task<IActionResult> Generate()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();

        var userId = Guid.Parse(userIdClaim.Value);

        var result = await service.GenerateQrTokenAsync(userId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
    [Authorize(Roles = "Admin,Staff")] 
    [HttpPost("verify")]
    public async Task<IActionResult> Verify([FromQuery] string token)
    {
        if (string.IsNullOrEmpty(token))
            return BadRequest("Token boş ola bilməz.");

        var result = await service.VerifyQrTokenAsync(token);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
