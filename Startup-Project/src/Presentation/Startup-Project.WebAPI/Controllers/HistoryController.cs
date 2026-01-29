using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Startup_Project.Application.Dtos.History;
using Startup_Project.Application.Enums;
using Startup_Project.Application.Services.Interfaces;

namespace Startup_Project.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HistoryController : ControllerBase
{
    private readonly IHistoryService _historyService;

    public HistoryController(IHistoryService historyService)
    {
        _historyService = historyService;
    }

    /// <summary>
    /// ?stifad?çinin tam tarixç?sini qaytar?r (filtrl?nmi? v? s?hif?l?nmi?)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetHistory(
        [FromQuery] ActivityStatus? status,
        [FromQuery] ActivityType? type,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var filter = new HistoryFilterDto
        {
            Status = status,
            ActivityType = type,
            StartDate = startDate,
            EndDate = endDate,
            Page = page,
            PageSize = pageSize
        };

        var result = await _historyService.GetMyHistoryAsync(filter);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// ?stifad?çinin f?aliyy?t xülas?sini qaytar?r
    /// </summary>
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var result = await _historyService.GetHistorySummaryAsync();

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// ?stifad?çinin son N f?aliyy?tini qaytar?r
    /// </summary>
    [HttpGet("recent")]
    public async Task<IActionResult> GetRecent([FromQuery] int count = 10)
    {
        var result = await _historyService.GetRecentActivitiesAsync(count);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Yaln?z öd?ni? tarixç?si
    /// </summary>
    [HttpGet("payments")]
    public async Task<IActionResult> GetPayments(
        [FromQuery] ActivityStatus? status,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var filter = new HistoryFilterDto
        {
            Status = status,
            ActivityType = ActivityType.Payment,
            StartDate = startDate,
            EndDate = endDate,
            Page = page,
            PageSize = pageSize
        };

        var result = await _historyService.GetMyHistoryAsync(filter);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Yaln?z check-in tarixç?si
    /// </summary>
    [HttpGet("check-ins")]
    public async Task<IActionResult> GetCheckIns(
        [FromQuery] ActivityStatus? status,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var filter = new HistoryFilterDto
        {
            Status = status,
            ActivityType = ActivityType.CheckIn,
            StartDate = startDate,
            EndDate = endDate,
            Page = page,
            PageSize = pageSize
        };

        var result = await _historyService.GetMyHistoryAsync(filter);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
