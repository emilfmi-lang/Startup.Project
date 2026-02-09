using Startup_Project.Application.Dtos.Common;
using Startup_Project.Application.Dtos.Favorites;
using Startup_Project.Application.Dtos.GymDtos;
using Startup_Project.Application.Models;

namespace Startup_Project.Application.Services.Interfaces;

public interface IFavoriteService
{
    // GYM FAVORITES
    Task<ResponseModel<bool>> ToggleGymFavoriteAsync(Guid gymId);

    
    Task<ResponseModel<PaginatedResult<FavoriteGymDto>>> GetFavoriteGymsAsync(int page = 1, int pageSize = 20);

    Task<ResponseModel<bool>> IsGymFavoriteAsync(Guid gymId);
    // TRAINER FAVORITES
    Task<ResponseModel<bool>> ToggleTrainerFavoriteAsync(Guid trainerId);
    Task<ResponseModel<PaginatedResult<FavoriteTrainerDto>>> GetFavoriteTrainersAsync(int page = 1, int pageSize = 20);
    Task<ResponseModel<bool>> IsTrainerFavoriteAsync(Guid trainerId);

    // COUNTS
    Task<ResponseModel<FavoriteCountsDto>> GetFavoriteCountsAsync();


}
