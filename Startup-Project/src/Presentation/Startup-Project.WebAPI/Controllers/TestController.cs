using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Startup_Project.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TestController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok("API is working!");
    }
}
