using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Startup_Project.Application.Services.Interfaces;

namespace Startup_Project.WebAPI.Controllers;

[ApiController]
[Route("api/favorites")]
[Authorize]
public class FavoritesController(IFavoriteService favoriteService) : ControllerBase
{
    #region -------------------- GYM FAVORITES --------------------

    [HttpPost("gyms/{gymId:guid}/toggle")]
    public async Task<IActionResult> ToggleGymFavorite(Guid gymId)
        => Ok(await favoriteService.ToggleGymFavoriteAsync(gymId));

    [HttpGet("gyms")]
    public async Task<IActionResult> GetFavoriteGyms([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(await favoriteService.GetFavoriteGymsAsync(page, pageSize));

    [HttpGet("gyms/{gymId:guid}/check")]
    public async Task<IActionResult> IsGymFavorite(Guid gymId)
        => Ok(await favoriteService.IsGymFavoriteAsync(gymId));

    #endregion

    #region -------------------- TRAINER FAVORITES --------------------

    [HttpPost("trainers/{trainerId:guid}/toggle")]
    public async Task<IActionResult> ToggleTrainerFavorite(Guid trainerId)
        => Ok(await favoriteService.ToggleTrainerFavoriteAsync(trainerId));

    [HttpGet("trainers")]
    public async Task<IActionResult> GetFavoriteTrainers([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(await favoriteService.GetFavoriteTrainersAsync(page, pageSize));

    [HttpGet("trainers/{trainerId:guid}/check")]
    public async Task<IActionResult> IsTrainerFavorite(Guid trainerId)
        => Ok(await favoriteService.IsTrainerFavoriteAsync(trainerId));

    #endregion

    #region -------------------- COUNTS --------------------

    [HttpGet("counts")]
    public async Task<IActionResult> GetFavoriteCounts()
        => Ok(await favoriteService.GetFavoriteCountsAsync());

    #endregion
}
