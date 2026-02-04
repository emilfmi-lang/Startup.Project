using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Startup_Project.Application.Dtos.Campaign;
using Startup_Project.Application.Services.Interfaces.Admin;

namespace Startup_Project.WebAPI.Controllers.Admin;

[Route("api/admin/campaigns")]
[ApiController]
[Authorize(Roles = "Admin")]
public class CampaignAdminController(ICampaignAdminService campaignAdminService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCampaignDto dto)
        => Ok(await campaignAdminService.CreateAsync(dto));

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(await campaignAdminService.GetAllAsync(page, pageSize));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
        => Ok(await campaignAdminService.GetByIdAsync(id));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCampaignDto dto)
        => Ok(await campaignAdminService.UpdateAsync(id, dto));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
        => Ok(await campaignAdminService.DeleteAsync(id));

    [HttpPut("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id)
        => Ok(await campaignAdminService.ActivateAsync(id));

    [HttpPut("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id)
        => Ok(await campaignAdminService.DeactivateAsync(id));

    [HttpPost("{id:guid}/rules")]
    public async Task<IActionResult> AddRule(Guid id, [FromBody] CreateCampaignRuleDto dto)
        => Ok(await campaignAdminService.AddRuleAsync(id, dto));

    [HttpDelete("{id:guid}/rules/{ruleId:guid}")]
    public async Task<IActionResult> RemoveRule(Guid id, Guid ruleId)
        => Ok(await campaignAdminService.RemoveRuleAsync(id, ruleId));

    [HttpGet("{id:guid}/usage")]
    public async Task<IActionResult> GetUsageHistory(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(await campaignAdminService.GetUsageHistoryAsync(id, page, pageSize));
}
