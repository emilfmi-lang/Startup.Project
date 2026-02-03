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
   [HttpGet]
    public async Task<IActionResult> GetHistory([FromQuery] HistoryFilterDto filter)
        => Ok(await historyService.GetMyHistoryAsync(filter));

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
        => Ok(await historyService.GetHistorySummaryAsync());

    [HttpGet("recent")]
    public async Task<IActionResult> GetRecent([FromQuery] int count = 10)
        => Ok(await historyService.GetRecentActivitiesAsync(count));

    [HttpGet("payments")]
    public async Task<IActionResult> GetPayments([FromQuery] HistoryFilterDto filter)
    {
        filter.ActivityType = ActivityType.Payment; 
        return Ok(await historyService.GetMyHistoryAsync(filter));
    }

    [HttpGet("check-ins")]
    public async Task<IActionResult> GetCheckIns([FromQuery] HistoryFilterDto filter)
    {
        filter.ActivityType = ActivityType.CheckIn; 
        return Ok(await historyService.GetMyHistoryAsync(filter));
    }
}
