using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Startup_Project.Application.Dtos.History;
using Startup_Project.Application.Enums;
using Startup_Project.Application.Services.Interfaces;

namespace Startup_Project.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HistoryController(IHistoryService historyService) : ControllerBase
{
    #region -------------------- YEN? ENDPOINTS --------------------

    /// Tam History s?hif?si data (Stats + Chart + Timeline + Achievements)
  
    /// <param name="period">week | month | year | all</param>
    [HttpGet("full")]
    public async Task<IActionResult> GetFullHistory([FromQuery] string period = "week")
        => Ok(await historyService.GetFullHistoryAsync(period));

    /// <summary>
    /// Activity Timeline (gün üzr? qrupla?d?r?lm??)
    /// </summary>
    [HttpGet("timeline")]
    public async Task<IActionResult> GetActivityTimeline(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
        => Ok(await historyService.GetActivityTimelineAsync(startDate, endDate, page, pageSize));

    /// Statistika kartlar? (4 kart)
  
    /// <param name="period">week | month | year</param>
    [HttpGet("stats")]
    public async Task<IActionResult> GetActivityStats([FromQuery] string period = "month")
        => Ok(await historyService.GetActivityStatsAsync(period));

    /// H?ft?lik aktivlik chart data
    /// </summary>
    [HttpGet("weekly-activity")]
    public async Task<IActionResult> GetWeeklyActivity()
        => Ok(await historyService.GetWeeklyActivityAsync());

    /// <summary>
    /// Ayl?q nailiyy?tl?r (3 kart)
    /// </summary>
    [HttpGet("achievements")]
    public async Task<IActionResult> GetMonthlyAchievements()
        => Ok(await historyService.GetMonthlyAchievementsAsync());

    #endregion

    #region -------------------- MÖVCUD ENDPOINTS --------------------

    /// <summary>
    /// Ümumi aktivlik tarixç?si
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetHistory([FromQuery] HistoryFilterDto filter)
        => Ok(await historyService.GetMyHistoryAsync(filter));

    /// <summary>
    /// Statistika xülas?si
    /// </summary>
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
        => Ok(await historyService.GetHistorySummaryAsync());

    /// <summary>
    /// Son aktivlikl?r
    /// </summary>
    [HttpGet("recent")]
    public async Task<IActionResult> GetRecent([FromQuery] int count = 10)
        => Ok(await historyService.GetRecentActivitiesAsync(count));

    /// <summary>
    /// Yaln?z öd?ni?l?r
    /// </summary>
    [HttpGet("payments")]
    public async Task<IActionResult> GetPayments([FromQuery] HistoryFilterDto filter)
    {
        filter.ActivityType = ActivityType.Payment;
        return Ok(await historyService.GetMyHistoryAsync(filter));
    }

    /// <summary>
    /// Yaln?z check-in-l?r
    /// </summary>
    [HttpGet("check-ins")]
    public async Task<IActionResult> GetCheckIns([FromQuery] HistoryFilterDto filter)
    {
        filter.ActivityType = ActivityType.CheckIn;
        return Ok(await historyService.GetMyHistoryAsync(filter));
    }

    /// Yaln?z PT sessiyalar
    [HttpGet("sessions")]
    public async Task<IActionResult> GetSessions([FromQuery] HistoryFilterDto filter)
    {
        filter.ActivityType = ActivityType.Session;
        return Ok(await historyService.GetMyHistoryAsync(filter));
    }

    #endregion
}
