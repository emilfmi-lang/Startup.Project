using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Startup_Project.Application.Dtos.TrainingSession;
using Startup_Project.Application.Services.Interfaces;

namespace Startup_Project.WebAPI.Controllers;

[Route("api/sessions/verify")]
[ApiController]
[Authorize(Roles = "Admin,Staff,GymOwner")]
public class SessionVerifyController : ControllerBase
{
    private readonly ITrainingSessionService _sessionService;

    public SessionVerifyController(ITrainingSessionService sessionService)
    {
        _sessionService = sessionService;
    }

    /// <summary>
    /// QR kodu skan et ve girisi tesdiqle
    /// Bu endpoint zal girisinde istifade olunur
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Verify([FromBody] SessionVerifyDto dto)
    {
        var result = await _sessionService.VerifyAndUseSessionAsync(dto);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }
}
