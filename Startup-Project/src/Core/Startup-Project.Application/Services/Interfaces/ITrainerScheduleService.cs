using Startup_Project.Application.Dtos.Trainer;
using Startup_Project.Application.Models;

namespace Startup_Project.Application.Services.Interfaces;

public interface ITrainerScheduleService
{
    Task<ResponseModel<WeeklyScheduleDto>> GetWeeklyScheduleAsync(DateTime? weekStart = null);
    Task<ResponseModel<TodayScheduleDto>> GetTodayScheduleAsync();
    Task<ResponseModel<ScheduleSessionItemDto>> CreateScheduleEntryAsync(CreateScheduleEntryDto request);
    Task<ResponseModel<ScheduleSessionItemDto>> UpdateScheduleEntryAsync(Guid id, UpdateScheduleEntryDto request);
    Task<ResponseModel<bool>> DeleteScheduleEntryAsync(Guid id);
    Task<ResponseModel<WeeklyScheduleDto>> BulkUpdateWeekAsync(BulkUpdateScheduleDto request);
}
