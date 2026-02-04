using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Startup_Project.Application.Dtos.TrainingSession;
using Startup_Project.Application.Services.Interfaces;

namespace Startup_Project.WebAPI.Controllers;

[Route("api/sessions")]
[ApiController]
public class TrainingSessionController(ITrainingSessionService sessionService) : ControllerBase
{
    [HttpPost("purchase")]
    [Authorize]
    public async Task<IActionResult> Purchase([FromBody] SessionPurchaseDto dto)
        => Ok(await sessionService.PurchaseSessionAsync(dto));

    [HttpGet("my")]
    [Authorize]
    public async Task<IActionResult> GetMySessions()
        => Ok(await sessionService.GetMySessionsAsync());

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetById(Guid id)
        => Ok(await sessionService.GetSessionByIdAsync(id));

    [HttpGet("{id:guid}/qr")]
    [Authorize]
    public async Task<IActionResult> GetQr(Guid id)
        => Ok(await sessionService.GetSessionQrAsync(id));

    [HttpPost("{id:guid}/cancel")]
    [Authorize]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] SessionCancelDto dto)
        => Ok(await sessionService.CancelSessionAsync(id, dto));
    [Authorize]
    [HttpPost("{id:guid}/confirm-payment")]
    public async Task<IActionResult> ConfirmPayment(Guid id, [FromBody] Guid paymentId)
        => Ok(await sessionService.ConfirmPaymentAsync(id, paymentId));
}
