using Startup_Project.Application.Dtos.Admin;
using Startup_Project.Application.Dtos.Common;
using Startup_Project.Application.Models;

namespace Startup_Project.Application.Services.Interfaces.Admin;

public interface IGymAdminPanelService
{
    Task<ResponseModel<GymAdminDashboardDto>> GetDashboardAsync();

    Task<ResponseModel<PaginatedResult<MemberListItemDto>>> GetMembersAsync(
        string? search = null,
        string? status = null,
        string? plan = null,
        int page = 1,
        int pageSize = 10);

    Task<ResponseModel<PaginatedResult<TrainerListItemDto>>> GetTrainersAsync(
        string? search = null,
        int page = 1,
        int pageSize = 10);

    /// <summary>
    /// Admin panelinden email gondermek
    /// </summary>
    Task<ResponseModel<string>> SendEmailAsync(SendEmailRequestDto request);
}
