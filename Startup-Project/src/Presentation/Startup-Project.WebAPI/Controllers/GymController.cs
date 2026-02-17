using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Startup_Project.Application.Dtos.GymDtos;
using Startup_Project.Application.Services.Interfaces;

namespace Startup_Project.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GymController(IGymService service) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllAsync()
        => Ok(await service.GetAllAsync());

    [HttpGet("list")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllWithFavoritesAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(await service.GetAllWithFavoritesAsync(page, pageSize));

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDetailAsync(Guid id)
        => Ok(await service.GetDetailAsync(id));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateAsync([FromBody] GymCreateDto dto)
        => Ok(await service.CreateAsync(dto));

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateAsync(string id, [FromBody] GymUpdateDto dto)
        => Ok(await service.UpdateAsync(id, dto));

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAsync(string id)
        => Ok(await service.DeleteAsync(id));
}
