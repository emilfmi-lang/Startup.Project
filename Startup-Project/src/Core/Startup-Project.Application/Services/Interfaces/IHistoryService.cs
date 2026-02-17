using Startup_Project.Application.Dtos.History;
using Startup_Project.Application.Models;

namespace Startup_Project.Application.Services.Interfaces;

public interface IHistoryService
{
    #region -------------------- MÖVCUD METODLAR --------------------

    /// <summary>
    /// Ümumi aktivlik tarixç?si
    /// </summary>
    Task<ResponseModel<HistoryResponseDto>> GetMyHistoryAsync(HistoryFilterDto filter);

    /// <summary>
    /// Statistika xülas?si
    /// </summary>
    Task<ResponseModel<HistorySummaryDto>> GetHistorySummaryAsync();

    /// <summary>
    /// Son aktivlikl?r
    /// </summary>
    Task<ResponseModel<List<UserActivityDto>>> GetRecentActivitiesAsync(int count = 10);

    #endregion

    #region -------------------- YEN? METODLAR --------------------

    /// <summary>
    /// Tam History s?hif?si üçün bütün data
    /// </summary>
    /// <param name="period">"week" | "month" | "year" | "all"</param>
    Task<ResponseModel<FullHistoryResponseDto>> GetFullHistoryAsync(string period = "week");

    /// <summary>
    /// Activity Timeline (gün üzr? qrupla?d?r?lm??)
    /// </summary>
    Task<ResponseModel<List<ActivityTimelineDto>>> GetActivityTimelineAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        int page = 1,
        int pageSize = 10);

    /// <summary>
    /// Statistika kartlar? (4 kart)
    /// </summary>
    /// <param name="period">"week" | "month" | "year"</param>
    Task<ResponseModel<ActivityStatsDto>> GetActivityStatsAsync(string period = "month");

    /// <summary>
    /// H?ft?lik aktivlik chart data
    /// </summary>
    Task<ResponseModel<List<WeeklyActivityDto>>> GetWeeklyActivityAsync();

    /// <summary>
    /// Ayl?q nailiyy?tl?r (3 kart)
    /// </summary>
    Task<ResponseModel<List<MonthlyAchievementDto>>> GetMonthlyAchievementsAsync();

    #endregion
}
