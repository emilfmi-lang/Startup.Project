using Startup_Project.Application.Dtos.GymDtos;
using Startup_Project.Application.Dtos.GymOwner;
using Startup_Project.Application.Dtos.Packages;
using Startup_Project.Application.Models;

namespace Startup_Project.Application.Services.Interfaces;

public interface IGymOwnerService
{
    Task<ResponseModel<MyGymDetailDto>> GetMyGymAsync();
    Task<ResponseModel<bool>> UpdateMyGymAsync(GymUpdateDto dto);
    Task<ResponseModel<MyGymStatsDto>> GetMyGymStatsAsync();
    Task<ResponseModel<List<GymMemberDto>>> GetMyGymMembersAsync();
    Task<ResponseModel<List<GymCheckInDto>>> GetTodayCheckInsAsync();
    Task<ResponseModel<List<PackageResponseDto>>> GetMyPackagesAsync();
}
