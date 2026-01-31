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
        => Ok(await service.GetAllAsync());

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByIdAsync(string id)
        => Ok(await service.GetByIdAsync(id));

    [HttpPost]
    [Authorize(Roles = "Admin,GymOwner")]
    public async Task<IActionResult> CreateAsync([FromBody] CreatePackageRequestDto dto)
        => Ok(await service.CreateAsync(dto));

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,GymOwner")]
    public async Task<IActionResult> UpdateAsync(string id, [FromBody] UpdatePackageRequestDto dto)
        => Ok(await service.UpdateAsync(id, dto));

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,GymOwner")]
    public async Task<IActionResult> DeleteAsync(string id)
        => Ok(await service.DeleteAsync(id));
}
