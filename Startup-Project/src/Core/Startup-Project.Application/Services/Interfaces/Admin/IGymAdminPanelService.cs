using Startup_Project.Application.Dtos.Admin;
using Startup_Project.Application.Dtos.Common;
using Startup_Project.Application.Models;

namespace Startup_Project.Application.Services.Interfaces.Admin;

/// <summary>
/// Gym Admin Panel Service - T?k zal?n admin paneli
/// JWT-d?n admin ID al?n?r ? admin-in zal? tap?l?r ? data qaytar?l?r
/// Frontend gymId bilm?y? ehtiyac yoxdur
/// </summary>
public interface IGymAdminPanelService
{
    /// <summary>
    /// Dashboard data: Gym info + stats
    /// Sidebar "Managing [GymName]" + Dashboard card-lar? üçün
    /// </summary>
    Task<ResponseModel<GymAdminDashboardDto>> GetDashboardAsync();

    /// <summary>
    /// Gym-in member-l?ri (paginated, search, filter)
    /// </summary>
    Task<ResponseModel<PaginatedResult<MemberListItemDto>>> GetMembersAsync(
        string? search = null,
        string? status = null,
        string? plan = null,
        int page = 1,
        int pageSize = 10);

    /// <summary>
    /// Gym-in trainer-l?ri (paginated, search)
    /// </summary>
    Task<ResponseModel<PaginatedResult<TrainerListItemDto>>> GetTrainersAsync(
        string? search = null,
        int page = 1,
        int pageSize = 10);
}
