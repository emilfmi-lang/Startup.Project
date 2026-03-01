using Startup_Project.Application.Dtos.Admin;
using Startup_Project.Application.Dtos.GymDtos;
using Startup_Project.Application.Models;

namespace Startup_Project.Application.Services.Interfaces.Admin;

public interface IAdminService
{
    Task<ResponseModel<DashboardStatsDto>> GetDashboardStatsAsync();
    Task<ResponseModel<List<UserListDto>>> GetAllUsersAsync();
    Task<ResponseModel<List<DeletedUserDto>>> GetDeletedUsersAsync();
    Task<ResponseModel<bool>> RestoreUserAsync(Guid userId);
    Task<ResponseModel<bool>> AssignRoleAsync(AssignRoleDto dto);
    Task<ResponseModel<bool>> RemoveRoleAsync(Guid userId, string roleName);
    Task<ResponseModel<bool>> BlockUserAsync(Guid userId);
    Task<ResponseModel<bool>> UnblockUserAsync(Guid userId);
    Task<ResponseModel<bool>> AssignGymOwnerAsync(AssignGymOwnerDto dto);
    Task<ResponseModel<List<GymGetDto>>> GetAllGymsAsync();
}
