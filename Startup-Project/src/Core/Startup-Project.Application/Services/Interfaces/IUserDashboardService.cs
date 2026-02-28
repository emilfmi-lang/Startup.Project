using Startup_Project.Application.Dtos.Dashboard;
using Startup_Project.Application.Models;

namespace Startup_Project.Application.Services.Interfaces;

public interface IUserDashboardService
{
    Task<ResponseModel<UserDashboardDto>> GetDashboardAsync();
}
