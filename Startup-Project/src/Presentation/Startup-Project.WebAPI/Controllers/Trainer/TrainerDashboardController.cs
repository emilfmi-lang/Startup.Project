using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Startup_Project.Application.Services.Interfaces;

namespace Startup_Project.WebAPI.Controllers.Trainer;

[Route("api/trainer/me/dashboard")]
[ApiController]
[Authorize(Roles = "Trainer")]
public class TrainerDashboardController(ITrainerDashboardService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetDashboard()
        => Ok(await service.GetDashboardAsync());
}
