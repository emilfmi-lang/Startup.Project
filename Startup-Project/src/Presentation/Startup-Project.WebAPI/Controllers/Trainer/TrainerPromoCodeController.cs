using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Startup_Project.Application.Dtos.PromoCode;
using Startup_Project.Application.Services.Interfaces;

namespace Startup_Project.WebAPI.Controllers.Trainer;

[Route("api/trainer/promo-codes")]
[ApiController]
[Authorize(Roles = "Trainer")]
public class TrainerPromoCodeController(ITrainerPromoCodeService service) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TrainerCreatePromoCodeDto dto)
        => Ok(await service.CreateAsync(dto));

    [HttpGet]
    public async Task<IActionResult> GetMyCodes([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(await service.GetMyPromoCodesAsync(page, pageSize));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
        => Ok(await service.GetByIdAsync(id));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePromoCodeDto dto)
        => Ok(await service.UpdateAsync(id, dto));

    [HttpPut("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id)
        => Ok(await service.ActivateAsync(id));

    [HttpPut("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id)
        => Ok(await service.DeactivateAsync(id));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
        => Ok(await service.DeleteAsync(id));
}
