using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Startup_Project.Application.Dtos.Trainer;
using Startup_Project.Application.Services.Interfaces;

namespace Startup_Project.WebAPI.Controllers.Trainer;

[Route("api/trainer/me/sessions")]
[ApiController]
[Authorize(Roles = "Trainer")]
public class TrainerSessionsController(ITrainerSessionService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetMySessions(
        [FromQuery] string? search = null,
        [FromQuery] string? status = null)
        => Ok(await service.GetMySessionsAsync(search, status));

    [HttpPost]
    public async Task<IActionResult> CreateSession([FromBody] TrainerCreateSessionDto request)
        => Ok(await service.CreateSessionAsync(request));

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromQuery] string status)
        => Ok(await service.UpdateSessionStatusAsync(id, status));
}
