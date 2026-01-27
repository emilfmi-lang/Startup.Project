using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Startup_Project.Application.Dtos.Packages;
using Startup_Project.Application.Services.Interfaces;

namespace Startup_Project.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PackageController(IPackageService service) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllAsync()
    {
        var result = await service.GetAllAsync();
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByIdAsync(string id)
    {
        var result = await service.GetByIdAsync(id);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,GymOwner")]
    public async Task<IActionResult> CreateAsync([FromBody] CreatePackageRequestDto dto)
    {
        var result = await service.CreateAsync(dto);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,GymOwner")]
    public async Task<IActionResult> UpdateAsync(string id, [FromBody] UpdatePackageRequestDto dto)
    {
        var result = await service.UpdateAsync(id, dto);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,GymOwner")]
    public async Task<IActionResult> DeleteAsync(string id)
    {
        var result = await service.DeleteAsync(id);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }
}
