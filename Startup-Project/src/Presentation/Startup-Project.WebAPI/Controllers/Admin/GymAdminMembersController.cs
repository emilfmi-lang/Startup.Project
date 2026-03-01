using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Startup_Project.Application.Dtos.Admin;
using Startup_Project.Application.Services.Interfaces.Admin;
using System.Security.Claims;

namespace Startup_Project.WebAPI.Controllers.Admin;

[Route("api/admin/members")]
[ApiController]
[Authorize(Roles = "GymOwner,Admin")]
public class GymAdminMembersController(
    IGymAdminMembersService service,
    IAdminService adminService) : ControllerBase
{
    /// <summary>
    /// GymOwner-? m?xsus gyms-l?rin siyah?s?n? getir
    /// </summary>
    [HttpGet("owned-gyms")]
    public async Task<IActionResult> GetOwnedGyms()
    {
        var ownerId = GetAdminId();
        var result = await service.GetOwnedGymsAsync(ownerId);
        return Ok(result);
    }

    /// <summary>
    /// Members siyah?s?n? getir (SelectedGymId il? filter ed? bil?c?yi)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMembers(
        [FromQuery] Guid? gymId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] string? plan = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortOrder = null)
    {
        var ownerId = GetAdminId();

        var filter = new AdminMembersFilterDto
        {
            SelectedGymId = gymId,
            Page = page,
            PageSize = pageSize,
            Search = search,
            Status = status,
            Plan = plan,
            SortBy = sortBy,
            SortOrder = sortOrder
        };

        var result = await service.GetMembersAsync(ownerId, filter);
        return Ok(result);
    }

    /// <summary>
    /// Member detail m?lumat? getir
    /// </summary>
    [HttpGet("{memberId:guid}")]
    public async Task<IActionResult> GetMemberDetail(Guid memberId)
    {
        var ownerId = GetAdminId();
        var result = await service.GetMemberDetailAsync(ownerId, memberId);
        return Ok(result);
    }

    /// <summary>
    /// Member-i trainer-? çevir
    /// gymId parametri verm?lidir ki, hans? gym-d? trainer olacaq
    /// </summary>
    [HttpPut("{memberId:guid}/make-trainer")]
    public async Task<IActionResult> MakeMemberTrainer(
        Guid memberId,
        [FromQuery] Guid gymId,
        [FromBody] MakeTrainerRequestDto request)
    {
        var ownerId = GetAdminId();
        var result = await service.MakeMemberTrainerAsync(ownerId, memberId, gymId, request);
        return Ok(result);
    }

    /// <summary>
    /// Trainers siyah?s?n? getir (gym-a ?sasl?)
    /// </summary>
    [HttpGet("trainers")]
    public async Task<IActionResult> GetTrainers(
        [FromQuery] Guid gymId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var ownerId = GetAdminId();
        var result = await service.GetTrainersAsync(ownerId, gymId, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Trainer detail m?lumat? getir
    /// </summary>
    [HttpGet("trainers/{trainerId:guid}")]
    public async Task<IActionResult> GetTrainerDetail(Guid trainerId)
    {
        var ownerId = GetAdminId();
        var result = await service.GetTrainerDetailAsync(ownerId, trainerId);
        return Ok(result);
    }

    // ???????????????????????? SEND MESSAGE ????????????????????????

    /// <summary>
    /// Admin/GymOwner t?r?find?n istifad?çiy? mesaj gönd?r (notification)
    /// </summary>
    [HttpPost("{memberId:guid}/send-message")]
    public async Task<IActionResult> SendMessageToUser(Guid memberId, [FromBody] AdminSendMessageDto dto)
    {
        var result = await adminService.SendMessageToUserAsync(memberId, dto);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    #region ==================== PRIVATE HELPERS ====================

    /// <summary>
    /// JWT token-d?n owner/admin ID-ni ç?xar
    /// </summary>
    private Guid GetAdminId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var ownerId))
            throw new UnauthorizedAccessException("Invalid owner ID");

        return ownerId;
    }

    #endregion
}
