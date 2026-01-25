using Startup_Project.Application.Dtos.GymOpeningHour;
using Startup_Project.Application.Models;

namespace Startup_Project.Application.Services.Interfaces;

public interface IGymOpeningHourService
{
    Task<ResponseModel<bool>> SetOpeningHoursAsync(Guid gymId, List<OpeningHourDto> dtos);
}
