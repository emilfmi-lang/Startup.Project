using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Startup_Project.Application.Dtos.Trainer;
using Startup_Project.Application.Dtos.TrainerSchedule;
using Startup_Project.Application.Services.Interfaces;

namespace Startup_Project.WebAPI.Controllers.Trainer;

[Route("api/trainer/me")]
[ApiController]
[Authorize(Roles = "Trainer")]
public class MyTrainerController : ControllerBase
{
    private readonly ITrainerService _trainerService;

    public MyTrainerController(ITrainerService trainerService)
    {
        _trainerService = trainerService;
    }
    [HttpGet]
    public async Task<IActionResult> GetMyProfile()
    {
        var result = await _trainerService.GetMyProfileAsync();
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateMyProfile([FromBody] TrainerUpdateDto dto)
    {
        var result = await _trainerService.UpdateMyProfileAsync(dto);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPut("schedule")]
    public async Task<IActionResult> UpdateMySchedule([FromBody] List<TrainerScheduleDto> schedules)
    {
        var result = await _trainerService.UpdateMyScheduleAsync(schedules);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }
}
