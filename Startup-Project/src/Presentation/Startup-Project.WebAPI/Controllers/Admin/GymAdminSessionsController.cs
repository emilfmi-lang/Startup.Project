using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Startup_Project.Application.Dtos.Admin;
using Startup_Project.Application.Services.Interfaces.Admin;

namespace Startup_Project.WebAPI.Controllers.Admin;

[Route("api/gym-admin")]
[ApiController]
[Authorize(Roles = "GymOwner")]
public class GymAdminSessionsController(IGymAdminSessionService service) : ControllerBase
{
    /// <summary>
    /// Mesqciye aid seanslari getir
    /// </summary>
    [HttpGet("trainers/{trainerId:guid}/sessions")]
    public async Task<IActionResult> GetTrainerSessions(Guid trainerId)
        => Ok(await service.GetTrainerSessionsAsync(trainerId));

    /// <summary>
    /// Yeni seans yarat
    /// </summary>
    [HttpPost("sessions")]
    public async Task<IActionResult> CreateSession([FromBody] CreateSessionDto request)
        => Ok(await service.CreateSessionAsync(request));

    /// <summary>
    /// Seans statusunu yenile (Completed / Cancelled)
    /// </summary>
    [HttpPatch("sessions/{id:guid}/status")]
    public async Task<IActionResult> UpdateSessionStatus(Guid id, [FromBody] UpdateSessionStatusDto request)
        => Ok(await service.UpdateSessionStatusAsync(id, request));
}
