using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Startup_Project.Application.Dtos.Trainer;
using Startup_Project.Application.Dtos.TrainerSchedule;
using Startup_Project.Application.Services.Interfaces;

namespace Startup_Project.WebAPI.Controllers.GymOwner;

[Route("api/owner/trainers")]
[ApiController]
[Authorize(Roles = "GymOwner,Admin")]
public class OwnerTrainerController : ControllerBase
{
    private readonly ITrainerService _trainerService;

    public OwnerTrainerController(ITrainerService trainerService)
    {
        _trainerService = trainerService;
    }

    /// <summary>
    /// Zalima yeni mesqci elave et
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TrainerCreateDto dto)
    {
        var result = await _trainerService.CreateTrainerAsync(dto);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Mesqci melumatlarini yenile
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] TrainerUpdateDto dto)
    {
        var result = await _trainerService.UpdateTrainerAsync(id, dto);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Mesqcini sil
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _trainerService.DeleteTrainerAsync(id);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Mesqcini aktiv et
    /// </summary>
    [HttpPut("{id}/activate")]
    public async Task<IActionResult> Activate(Guid id)
    {
        var result = await _trainerService.ActivateTrainerAsync(id);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Mesqcini deaktiv et
    /// </summary>
    [HttpPut("{id}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var result = await _trainerService.DeactivateTrainerAsync(id);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Mesqcinin is cedvelini teyin et
    /// </summary>
    [HttpPut("{id}/schedule")]
    public async Task<IActionResult> SetSchedule(Guid id, [FromBody] List<TrainerScheduleDto> schedules)
    {
        var result = await _trainerService.SetTrainerScheduleAsync(id, schedules);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }
}
