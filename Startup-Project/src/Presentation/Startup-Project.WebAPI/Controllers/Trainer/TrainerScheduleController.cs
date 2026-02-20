using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Startup_Project.Application.Dtos.Trainer;
using Startup_Project.Application.Services.Interfaces;

namespace Startup_Project.WebAPI.Controllers.Trainer;

[Route("api/trainer/me/schedule")]
[ApiController]
[Authorize(Roles = "Trainer")]
public class TrainerScheduleController(ITrainerScheduleService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetWeeklySchedule([FromQuery] DateTime? weekStart = null)
        => Ok(await service.GetWeeklyScheduleAsync(weekStart));

    [HttpGet("today")]
    public async Task<IActionResult> GetTodaySchedule()
        => Ok(await service.GetTodayScheduleAsync());

    [HttpPost]
    public async Task<IActionResult> CreateEntry([FromBody] CreateScheduleEntryDto request)
        => Ok(await service.CreateScheduleEntryAsync(request));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateEntry(Guid id, [FromBody] UpdateScheduleEntryDto request)
        => Ok(await service.UpdateScheduleEntryAsync(id, request));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteEntry(Guid id)
        => Ok(await service.DeleteScheduleEntryAsync(id));

    [HttpPut("week")]
    public async Task<IActionResult> BulkUpdateWeek([FromBody] BulkUpdateScheduleDto request)
        => Ok(await service.BulkUpdateWeekAsync(request));
}
