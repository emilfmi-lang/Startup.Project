using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Startup_Project.Application.Services.Interfaces;

namespace Startup_Project.WebAPI.Controllers;

[Route("api/user/subscriptions")]
[ApiController]
[Authorize]
public class UserSubscriptionController(IUserSubscriptionService service) : ControllerBase
{
    /// <summary>
    /// Cari istifad?çinin aktiv abun?likl?rini g?tirir
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMyActiveSubscriptionsAsync()
        => Ok(await service.GetMyActiveSubscriptionsAsync());

    /// <summary>
    /// Cari istifad?çinin öd?ni? tarixç?sini g?tirir
    /// </summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetMyPaymentHistoryAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(await service.GetMyPaymentHistoryAsync(page, pageSize));

    /// <summary>
    /// Abun?liyi yenil?yir (renew)
    /// </summary>
    [HttpPost("{id:guid}/renew")]
    public async Task<IActionResult> RenewSubscriptionAsync(Guid id)
        => Ok(await service.RenewSubscriptionAsync(id));
}
