using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Startup_Project.Application.Services.Interfaces;

namespace Startup_Project.WebAPI.Controllers;

[Route("api/user/sessions")]
[ApiController]
[Authorize]
public class UserSessionController(IUserSessionService service) : ControllerBase
{
    /// <summary>
    /// "Book New" tab — bütün mövcud g?l?c?k sessiyalar.
    /// </summary>
    [HttpGet("available")]
    public async Task<IActionResult> GetAvailableSessions(
        [FromQuery] string? search = null,
        [FromQuery] string? exerciseType = null,
        [FromQuery] Guid? trainerId = null)
        => Ok(await service.GetAvailableSessionsAsync(search, exerciseType, trainerId));

    /// <summary>
    /// "Upcoming" tab — user-?n qeyd? ald??? g?l?c?k sessiyalar.
    /// </summary>
    [HttpGet("upcoming")]
    public async Task<IActionResult> GetMyUpcomingSessions()
        => Ok(await service.GetMyUpcomingSessionsAsync());

    /// <summary>
    /// "Past" tab — user-?n keçmi? sessiyalar?.
    /// </summary>
    [HttpGet("past")]
    public async Task<IActionResult> GetMyPastSessions()
        => Ok(await service.GetMyPastSessionsAsync());

    /// <summary>
    /// Sessiya l??v etm?.
    /// </summary>
    [HttpPost("{sessionId:guid}/cancel")]
    public async Task<IActionResult> CancelBooking(Guid sessionId)
        => Ok(await service.CancelBookingAsync(sessionId));
}
