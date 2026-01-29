using Startup_Project.Application.Dtos.GymDtos;
using Startup_Project.Application.Models;

namespace Startup_Project.Application.Services.Interfaces;

public interface IFavoriteService
{
    /// <summary>
    /// Toggle favorite: ?g?r zal art?q favoritd?dirs? silir, yoxdursa ?lav? edir
    /// </summary>
    /// <returns>true = ?lav? edildi, false = silindi</returns>
    Task<ResponseModel<bool>> ToggleFavoriteAsync(Guid gymId);

    /// <summary>
    /// Cari istifad?çinin sevimli zallar?n? g?tirir (pagination il?)
    /// </summary>
    Task<ResponseModel<List<GymListDto>>> GetUserFavoritesAsync(int page = 1, int pageSize = 20);

    /// <summary>
    /// Bu zal cari istifad?çinin sevimlisind?dir mi?
    /// </summary>
    Task<ResponseModel<bool>> IsFavoriteAsync(Guid gymId);

    /// <summary>
    /// Zal? neç? n?f?r sevimliy? ?lav? edib
    /// </summary>
    Task<ResponseModel<int>> GetFavoriteCountAsync(Guid gymId);
}
