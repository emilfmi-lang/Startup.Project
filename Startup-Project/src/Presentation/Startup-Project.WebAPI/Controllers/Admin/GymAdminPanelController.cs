using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Startup_Project.Application.Services.Interfaces.Admin;

namespace Startup_Project.WebAPI.Controllers.Admin;

/// <summary>
/// Gym Admin Panel Controller
/// T?k zal?n admin paneli üçün sad?l??dirilmi? endpoint-l?r
/// JWT-d?n admin tap?l?r ? admin-in zal? avtomatik tap?l?r
/// Frontend gymId bilm?y? ehtiyac yoxdur
/// </summary>
[Route("api/gym-admin")]
[ApiController]
[Authorize(Roles = "GymOwner")]
public class GymAdminPanelController(IGymAdminPanelService service) : ControllerBase
{
    /// <summary>
    /// Dashboard: Gym info + stats
    /// Sidebar "Managing [GymName]" + Dashboard card-lar?
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
        => Ok(await service.GetDashboardAsync());

    /// <summary>
    /// Bu zal?n member-l?ri (paginated + search + filter)
    /// </summary>
    [HttpGet("members")]
    public async Task<IActionResult> GetMembers(
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] string? plan = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
        => Ok(await service.GetMembersAsync(search, status, plan, page, pageSize));

    /// <summary>
    /// Bu zal?n trainer-l?ri (paginated + search)
    /// </summary>
    [HttpGet("trainers")]
    public async Task<IActionResult> GetTrainers(
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
        => Ok(await service.GetTrainersAsync(search, page, pageSize));
}
