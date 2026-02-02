using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Startup_Project.Application.Dtos.PromoCode;
using Startup_Project.Application.Services.Interfaces.Admin;

namespace Startup_Project.WebAPI.Controllers.Admin;

[Route("api/admin/promo-codes")]
[ApiController]
[Authorize(Roles = "Admin")]
public class PromoCodeAdminController(IPromoCodeAdminService promoCodeAdminService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePromoCodeDto dto)
        => Ok(await promoCodeAdminService.CreateAsync(dto));

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(await promoCodeAdminService.GetAllAsync(page, pageSize));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
        => Ok(await promoCodeAdminService.GetByIdAsync(id));

    [HttpGet("by-code/{code}")]
    public async Task<IActionResult> GetByCode(string code)
        => Ok(await promoCodeAdminService.GetByCodeAsync(code));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePromoCodeDto dto)
        => Ok(await promoCodeAdminService.UpdateAsync(id, dto));

    [HttpPut("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id)
        => Ok(await promoCodeAdminService.DeactivateAsync(id));

    [HttpPut("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id)
        => Ok(await promoCodeAdminService.ActivateAsync(id));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
        => Ok(await promoCodeAdminService.DeleteAsync(id));

    [HttpGet("{id:guid}/usage")]
    public async Task<IActionResult> GetUsageHistory(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(await promoCodeAdminService.GetUsageHistoryAsync(id, page, pageSize));
}
