using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Startup_Project.Application.Services.Interfaces;

namespace Startup_Project.WebAPI.Controllers;

[ApiController]
[Route("api/favorites")]
[Authorize]
public class FavoritesController(IFavoriteService favoriteService) : ControllerBase
{
    #region ??????????????????? GYM FAVORITES ???????????????????

    /// <summary>
    /// Gym-i sevimlil?r? ?lav? et / sil (Toggle)
    /// </summary>
    /// <returns>true = ?lav? edildi, false = silindi</returns>
    [HttpPost("gyms/{gymId:guid}/toggle")]
    public async Task<IActionResult> ToggleGymFavorite(Guid gymId)
        => Ok(await favoriteService.ToggleGymFavoriteAsync(gymId));

    /// <summary>
    /// ?stifad?çinin sevimli gym-l?rini g?tirir (Gyms tab)
    /// </summary>
    [HttpGet("gyms")]
    public async Task<IActionResult> GetFavoriteGyms([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(await favoriteService.GetFavoriteGymsAsync(page, pageSize));

    /// <summary>
    /// Bu gym favorit-d?dir?
    /// </summary>
    [HttpGet("gyms/{gymId:guid}/check")]
    public async Task<IActionResult> IsGymFavorite(Guid gymId)
        => Ok(await favoriteService.IsGymFavoriteAsync(gymId));

    #endregion

    #region ??????????????????? TRAINER FAVORITES ???????????????????

    /// <summary>
    /// Trainer-i sevimlil?r? ?lav? et / sil (Toggle)
    /// </summary>
    /// <returns>true = ?lav? edildi, false = silindi</returns>
    [HttpPost("trainers/{trainerId:guid}/toggle")]
    public async Task<IActionResult> ToggleTrainerFavorite(Guid trainerId)
        => Ok(await favoriteService.ToggleTrainerFavoriteAsync(trainerId));

    /// <summary>
    /// ?stifad?çinin sevimli trainer-l?rini g?tirir (Trainers tab)
    /// </summary>
    [HttpGet("trainers")]
    public async Task<IActionResult> GetFavoriteTrainers([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(await favoriteService.GetFavoriteTrainersAsync(page, pageSize));

    /// <summary>
    /// Bu trainer favorit-d?dir?
    /// </summary>
    [HttpGet("trainers/{trainerId:guid}/check")]
    public async Task<IActionResult> IsTrainerFavorite(Guid trainerId)
        => Ok(await favoriteService.IsTrainerFavoriteAsync(trainerId));

    #endregion

    #region ??????????????????? COUNTS ???????????????????

    /// <summary>
    /// Tab badge-l?r üçün say - "Gyms (3)" "Trainers (3)"
    /// </summary>
    [HttpGet("counts")]
    public async Task<IActionResult> GetFavoriteCounts()
        => Ok(await favoriteService.GetFavoriteCountsAsync());

    #endregion
}
