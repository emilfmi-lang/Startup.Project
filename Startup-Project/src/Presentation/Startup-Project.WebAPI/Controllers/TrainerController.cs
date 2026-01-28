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

    /// <summary>
    /// Butun aktiv mesqcileri getir
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var result = await _trainerService.GetAllTrainersAsync();
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Mesqci profilini getir
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _trainerService.GetTrainerByIdAsync(id);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Zalin mesqcilerini getir
    /// </summary>
    [HttpGet("gym/{gymId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByGym(Guid gymId)
    {
        var result = await _trainerService.GetTrainersByGymAsync(gymId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Mesqcinin reviewlarini getir
    /// </summary>
    [HttpGet("{id}/reviews")]
    [AllowAnonymous]
    public async Task<IActionResult> GetReviews(Guid id)
    {
        var result = await _trainerService.GetTrainerReviewsAsync(id);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Mesqcinin is cedvelini getir
    /// </summary>
    [HttpGet("{id}/schedule")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSchedule(Guid id)
    {
        var result = await _trainerService.GetTrainerScheduleAsync(id);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Mesqciye review yaz
    /// </summary>
    [HttpPost("{id}/reviews")]
    [Authorize]
    public async Task<IActionResult> AddReview(Guid id, [FromBody] TrainerReviewCreateDto dto)
    {
        var result = await _trainerService.AddReviewAsync(id, dto);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }
}
