using Startup_Project.Application.Dtos.Trainer;
using Startup_Project.Application.Models;

namespace Startup_Project.Application.Services.Interfaces;

public interface ITrainerDashboardService
{
    Task<ResponseModel<TrainerDashboardDto>> GetDashboardAsync();
}
