using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Startup_Project.Application.Services.Interfaces;

namespace Startup_Project.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FavoritesController(IFavoriteService favoriteService) : ControllerBase
{
    /// <summary>
    /// Zal? sevimlil?r? ?lav? et v? ya sil (Toggle)
    /// </summary>
    /// <returns>true = ?lav? edildi, false = silindi</returns>
    [HttpPost("{gymId:guid}/toggle")]
    [Authorize]
    public async Task<IActionResult> ToggleFavorite(Guid gymId)
    {
        var result = await favoriteService.ToggleFavoriteAsync(gymId);
        return Ok(result);
    }

    /// <summary>
    /// Cari istifad?çinin sevimli zallar?n? g?tirir
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetUserFavorites([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await favoriteService.GetUserFavoritesAsync(page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Bu zal cari istifad?çinin sevimlisind?dir mi?
    /// </summary>
    [HttpGet("check/{gymId:guid}")]
    [Authorize]
    public async Task<IActionResult> CheckFavorite(Guid gymId)
    {
        var result = await favoriteService.IsFavoriteAsync(gymId);
        return Ok(result);
    }

    /// <summary>
    /// Zal? neç? n?f?r sevimliy? ?lav? edib (Public)
    /// </summary>
    [HttpGet("count/{gymId:guid}")]
    public async Task<IActionResult> GetFavoriteCount(Guid gymId)
    {
        var result = await favoriteService.GetFavoriteCountAsync(gymId);
        return Ok(result);
    }
}
