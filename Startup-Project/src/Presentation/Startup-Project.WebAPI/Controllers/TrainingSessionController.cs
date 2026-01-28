using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Startup_Project.Application.Dtos.TrainingSession;
using Startup_Project.Application.Services.Interfaces;

namespace Startup_Project.WebAPI.Controllers;

[Route("api/sessions")]
[ApiController]
public class TrainingSessionController : ControllerBase
{
    private readonly ITrainingSessionService _sessionService;

    public TrainingSessionController(ITrainingSessionService sessionService)
    {
        _sessionService = sessionService;
    }

    /// <summary>
    /// Sessiya satin al (mesqci ile bir gedislik)
    /// </summary>
    [HttpPost("purchase")]
    [Authorize]
    public async Task<IActionResult> Purchase([FromBody] SessionPurchaseDto dto)
    {
        var result = await _sessionService.PurchaseSessionAsync(dto);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Menim sessiyalarim
    /// </summary>
    [HttpGet("my")]
    [Authorize]
    public async Task<IActionResult> GetMySessions()
    {
        var result = await _sessionService.GetMySessionsAsync();
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Sessiya detali
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _sessionService.GetSessionByIdAsync(id);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Sessiyanin QR kodunu al
    /// </summary>
    [HttpGet("{id}/qr")]
    [Authorize]
    public async Task<IActionResult> GetQr(Guid id)
    {
        var result = await _sessionService.GetSessionQrAsync(id);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Sessiyani legv et
    /// </summary>
    [HttpPost("{id}/cancel")]
    [Authorize]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] SessionCancelDto dto)
    {
        var result = await _sessionService.CancelSessionAsync(id, dto);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Odenis tesdiqle (callback)
    /// </summary>
    [HttpPost("{id}/confirm-payment")]
    public async Task<IActionResult> ConfirmPayment(Guid id, [FromBody] Guid paymentId)
    {
        var result = await _sessionService.ConfirmPaymentAsync(id, paymentId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }
}
