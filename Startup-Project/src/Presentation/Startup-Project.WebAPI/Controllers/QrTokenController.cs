using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Startup_Project.Application.Dtos.QrCode;
using Startup_Project.Application.Services.Interfaces;
using System.Security.Claims;

namespace Startup_Project.WebAPI.Controllers;

[Route("api/qr")]
[ApiController]
[Authorize]
public class QrTokenController(IQrTokenService service) : ControllerBase
{
    private Guid GetUserId() => Guid.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException());

    /// <summary>
    /// Spesifik gym üçün QR code generate et
    /// </summary>
    [HttpGet("my-qr")]
    public async Task<IActionResult> GenerateQrCode([FromQuery] Guid gymId)
    {
        var result = await service.GenerateQrTokenAsync(GetUserId(), gymId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Bütün aktiv subscription-lar üçün QR code-ları gətir
    /// </summary>
    [HttpGet("my-qr/active")]
    public async Task<IActionResult> GetAllActiveQrCodes()
    {
        var result = await service.GetMyActiveQrCodesAsync(GetUserId());
        return Ok(result);
    }

    /// <summary>
    /// User-in öz check-in tarixçəsi
    /// </summary>
    [HttpGet("checkin-history")]
    public async Task<IActionResult> GetMyCheckInHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await service.GetMyCheckInHistoryAsync(GetUserId(), page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Gym staff/owner QR kodu verify edir — check-in qeydə alınır
    /// </summary>
    [HttpPost("verify")]
    [Authorize(Roles = "Admin,GymOwner")]
    public async Task<IActionResult> VerifyQrCode([FromBody] QrVerifyRequestDto request)
    {
        var result = await service.VerifyQrTokenAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Check-out qeydə al
    /// </summary>
    [HttpPost("checkout/{checkInLogId:guid}")]
    [Authorize(Roles = "Admin,GymOwner")]
    public async Task<IActionResult> CheckOut(Guid checkInLogId)
    {
        var result = await service.CheckOutAsync(checkInLogId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Gym-in bugünkü check-in-ləri (gym owner üçün)
    /// </summary>
    [HttpGet("gym/{gymId:guid}/checkins")]
    [Authorize(Roles = "Admin,GymOwner")]
    public async Task<IActionResult> GetGymCheckIns(
        Guid gymId,
        [FromQuery] DateTime? date = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var result = await service.GetGymCheckInsAsync(gymId, date, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Gym check-in statistikası
    /// </summary>
    [HttpGet("gym/{gymId:guid}/checkin-stats")]
    [Authorize(Roles = "Admin,GymOwner")]
    public async Task<IActionResult> GetGymCheckInStats(Guid gymId)
    {
        var result = await service.GetGymCheckInStatsAsync(gymId);
        return Ok(result);
    }
}
