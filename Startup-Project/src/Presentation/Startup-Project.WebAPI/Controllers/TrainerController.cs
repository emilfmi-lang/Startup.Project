using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Startup_Project.Application.Dtos.Trainer;
using Startup_Project.Application.Dtos.TrainerReview;
using Startup_Project.Application.Services.Interfaces;

namespace Startup_Project.WebAPI.Controllers;

[Route("api/trainers")]
[ApiController]
public class TrainerController(ITrainerService trainerService) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
        => Ok(await trainerService.GetAllTrainersAsync());

    [HttpGet("online")]
    [AllowAnonymous]
    public async Task<IActionResult> GetOnlineTrainers()
        => Ok(await trainerService.GetOnlineTrainersAsync());

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id)
        => Ok(await trainerService.GetTrainerByIdAsync(id));

    [HttpGet("gym/{gymId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByGym(Guid gymId)
        => Ok(await trainerService.GetTrainersByGymAsync(gymId));

    [HttpGet("{id}/reviews")]
    [AllowAnonymous]
    public async Task<IActionResult> GetReviews(Guid id)
        => Ok(await trainerService.GetTrainerReviewsAsync(id));

    [HttpGet("{id}/schedule")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSchedule(Guid id)
        => Ok(await trainerService.GetTrainerScheduleAsync(id));

    [HttpPost("{id}/reviews")]
    [Authorize]
    public async Task<IActionResult> AddReview(Guid id, [FromBody] TrainerReviewCreateDto dto)
        => Ok(await trainerService.AddReviewAsync(id, dto));

    [HttpPost("apply-online")]
    [Authorize]
    public async Task<IActionResult> ApplyAsOnlineTrainer([FromBody] OnlineTrainerApplyDto dto)
        => Ok(await trainerService.ApplyAsOnlineTrainerAsync(dto));
}
