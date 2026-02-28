using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Startup_Project.Application.Services.Interfaces;

namespace Startup_Project.WebAPI.Controllers;

[Route("api/user/dashboard")]
[ApiController]
[Authorize]
public class UserDashboardController(IUserDashboardService service) : ControllerBase
{
    /// <summary>
    /// User panel Dashboard — Active Subscription + This Month Stats + Recent Activity
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetDashboard()
        => Ok(await service.GetDashboardAsync());
}
