using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Startup_Project.Application.Services.Interfaces;

namespace Startup_Project.WebAPI.Controllers.Trainer;

[Route("api/trainer/sessions")]
[ApiController]
[Authorize(Roles = "Trainer")]
public class TrainerSessionController : ControllerBase
{
    private readonly ITrainingSessionService _sessionService;

    public TrainerSessionController(ITrainingSessionService sessionService)
    {
        _sessionService = sessionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMySessions()
    {
        var result = await _sessionService.GetTrainerSessionsAsync();
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }
}
