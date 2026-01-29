using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Startup_Project.Application.Dtos.Trainer;
using Startup_Project.Application.Dtos.TrainerReview;
using Startup_Project.Application.Services.Interfaces;

namespace Startup_Project.WebAPI.Controllers;

[Route("api/trainers")]
[ApiController]
public class TrainerController : ControllerBase
{
    private readonly ITrainerService _trainerService;

    public TrainerController(ITrainerService trainerService)
    {
        _trainerService = trainerService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var result = await _trainerService.GetAllTrainersAsync();
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("online")]
    [AllowAnonymous]
    public async Task<IActionResult> GetOnlineTrainers()
    {
        var result = await _trainerService.GetOnlineTrainersAsync();
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _trainerService.GetTrainerByIdAsync(id);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("gym/{gymId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByGym(Guid gymId)
    {
        var result = await _trainerService.GetTrainersByGymAsync(gymId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("{id}/reviews")]
    [AllowAnonymous]
    public async Task<IActionResult> GetReviews(Guid id)
    {
        var result = await _trainerService.GetTrainerReviewsAsync(id);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("{id}/schedule")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSchedule(Guid id)
    {
        var result = await _trainerService.GetTrainerScheduleAsync(id);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("{id}/reviews")]
    [Authorize]
    public async Task<IActionResult> AddReview(Guid id, [FromBody] TrainerReviewCreateDto dto)
    {
        var result = await _trainerService.AddReviewAsync(id, dto);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("apply-online")]
    [Authorize]
    public async Task<IActionResult> ApplyAsOnlineTrainer([FromBody] OnlineTrainerApplyDto dto)
    {
        var result = await _trainerService.ApplyAsOnlineTrainerAsync(dto);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }
}
