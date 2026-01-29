using Startup_Project.Application.Dtos.History;
using Startup_Project.Application.Models;

namespace Startup_Project.Application.Services.Interfaces;

public interface IHistoryService
{
    Task<ResponseModel<HistoryResponseDto>> GetMyHistoryAsync(HistoryFilterDto filter);
    Task<ResponseModel<HistorySummaryDto>> GetHistorySummaryAsync();
    Task<ResponseModel<List<UserActivityDto>>> GetRecentActivitiesAsync(int count = 10);
}
