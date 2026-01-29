using Startup_Project.Application.Dtos.GymDtos;
using Startup_Project.Application.Models;

namespace Startup_Project.Application.Services.Interfaces;

public interface IGymService
{
    Task<ResponseModel<bool>> CreateAsync(GymCreateDto dto);
    Task<ResponseModel<bool>> UpdateAsync(string id, GymUpdateDto dto);
    Task<ResponseModel<List<GymGetDto>>> GetAllAsync();
    Task<ResponseModel<List<GymListDto>>> GetAllWithFavoritesAsync(int page = 1, int pageSize = 20);
    Task<ResponseModel<bool>> DeleteAsync(string id);
}
