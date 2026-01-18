using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Startup_Project.Application.Dtos.Account;
using Startup_Project.Application.Services.Interfaces.Account;

namespace Startup_Project.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController(IAccountService accountService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegisterDto dto)
    {
        var result = await accountService.RegisterAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
    {
        var result = await accountService.LoginAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
