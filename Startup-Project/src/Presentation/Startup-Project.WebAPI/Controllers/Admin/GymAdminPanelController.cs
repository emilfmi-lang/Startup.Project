using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Startup_Project.Application.Dtos.Admin;
using Startup_Project.Application.Services.Interfaces.Admin;

namespace Startup_Project.WebAPI.Controllers.Admin;

[Route("api/gym-admin")]
[ApiController]
[Authorize(Roles = "GymOwner")]
public class GymAdminPanelController(IGymAdminPanelService service) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
        => Ok(await service.GetDashboardAsync());

    [HttpGet("members")]
    public async Task<IActionResult> GetMembers(
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] string? plan = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
        => Ok(await service.GetMembersAsync(search, status, plan, page, pageSize));

    [HttpGet("trainers")]
    public async Task<IActionResult> GetTrainers(
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
        => Ok(await service.GetTrainersAsync(search, page, pageSize));

    [HttpPost("send-email")]
    public async Task<IActionResult> SendEmail([FromBody] SendEmailRequestDto request)
        => Ok(await service.SendEmailAsync(request));
}
