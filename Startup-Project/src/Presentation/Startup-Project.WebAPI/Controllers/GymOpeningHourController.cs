using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Startup_Project.Application.Dtos.GymOpeningHour;
using Startup_Project.Application.Services.Interfaces;

namespace Startup_Project.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GymOpeningHourController(IGymOpeningHourService service) : ControllerBase
{
    [HttpPost("SetOpeningHours/{gymId:guid}")]
    [Authorize(Roles = "Admin,GymOwner")]
    public async Task<IActionResult> SetOpeningHoursAsync([FromRoute] Guid gymId, [FromBody] List<OpeningHourDto> dtos)
        => Ok(await service.SetOpeningHoursAsync(gymId, dtos));

    [HttpGet("{gymId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetOpeningHoursAsync([FromRoute] Guid gymId)
        => Ok(await service.GetOpeningHoursAsync(gymId));
}