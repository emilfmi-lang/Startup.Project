using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Startup_Project.Application.Dtos.GymDtos;
using Startup_Project.Application.Services.Interfaces;

namespace Startup_Project.WebAPI.Controllers.GymOwner;

[Route("api/owner")]
[ApiController]
[Authorize(Roles = "GymOwner,Admin")]
public class MyGymController : ControllerBase
{
    private readonly IGymOwnerService _gymOwnerService;
    private readonly ITrainingSessionService _sessionService;

    public MyGymController(IGymOwnerService gymOwnerService, ITrainingSessionService sessionService)
    {
        _gymOwnerService = gymOwnerService;
        _sessionService = sessionService;
    }

    [HttpGet("my-gym")]
    public async Task<IActionResult> GetMyGym()
    {
        var result = await _gymOwnerService.GetMyGymAsync();
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpPut("my-gym")]
    public async Task<IActionResult> UpdateMyGym([FromBody] GymUpdateDto dto)
    {
        var result = await _gymOwnerService.UpdateMyGymAsync(dto);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("my-gym/stats")]
    public async Task<IActionResult> GetMyGymStats()
    {
        var result = await _gymOwnerService.GetMyGymStatsAsync();
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("members")]
    public async Task<IActionResult> GetMyGymMembers()
    {
        var result = await _gymOwnerService.GetMyGymMembersAsync();
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("check-ins/today")]
    public async Task<IActionResult> GetTodayCheckIns()
    {
        var result = await _gymOwnerService.GetTodayCheckInsAsync();
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("packages")]
    public async Task<IActionResult> GetMyPackages()
    {
        var result = await _gymOwnerService.GetMyPackagesAsync();
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Zalimin butun training sessiyalari
    /// </summary>
    [HttpGet("sessions")]
    public async Task<IActionResult> GetGymSessions()
    {
        var result = await _sessionService.GetGymSessionsAsync();
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }
}
