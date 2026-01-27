using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Startup_Project.Application.Dtos.Admin;
using Startup_Project.Application.Dtos.GymDtos;
using Startup_Project.Application.Services.Interfaces.Admin;

namespace Startup_Project.WebAPI.Controllers.Admin;

[Route("api/admin")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var result = await _adminService.GetDashboardStatsAsync();
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var result = await _adminService.GetAllUsersAsync();
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("users/assign-role")]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto dto)
    {
        var result = await _adminService.AssignRoleAsync(dto);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("users/{userId}/roles/{roleName}")]
    public async Task<IActionResult> RemoveRole(Guid userId, string roleName)
    {
        var result = await _adminService.RemoveRoleAsync(userId, roleName);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpPut("users/{userId}/block")]
    public async Task<IActionResult> BlockUser(Guid userId)
    {
        var result = await _adminService.BlockUserAsync(userId);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpPut("users/{userId}/unblock")]
    public async Task<IActionResult> UnblockUser(Guid userId)
    {
        var result = await _adminService.UnblockUserAsync(userId);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("gyms/assign-owner")]
    public async Task<IActionResult> AssignGymOwner([FromBody] AssignGymOwnerDto dto)
    {
        var result = await _adminService.AssignGymOwnerAsync(dto);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("gyms")]
    public async Task<IActionResult> GetAllGyms()
    {
        var result = await _adminService.GetAllGymsAsync();
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }
}
