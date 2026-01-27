using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Startup_Project.Application.Dtos.GymOpeningHour;
using Startup_Project.Application.Services.Interfaces;

namespace Startup_Project.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GymOpeningHourController(IGymOpeningHourService service) : ControllerBase
{
    [HttpPost("SetOpeningHours/{gymId}")]
    [Authorize(Roles = "Admin,GymOwner")]
    public async Task<IActionResult> SetOpeningHoursAsync([FromRoute] Guid gymId, [FromBody] List<OpeningHourDto> dtos)
    {
        var result = await service.SetOpeningHoursAsync(gymId, dtos);
        if (result.Success)
            return Ok(result);
        return BadRequest(result);
    }

    [HttpGet("{gymId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetOpeningHoursAsync([FromRoute] Guid gymId)
    {
        var result = await service.GetOpeningHoursAsync(gymId);
        if (result.Success)
            return Ok(result);
        return BadRequest(result);
    }
}