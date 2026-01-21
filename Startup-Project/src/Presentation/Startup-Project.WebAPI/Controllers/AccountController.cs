using Microsoft.AspNetCore.Mvc;
using Startup_Project.Application.Dtos.Account;
using Startup_Project.Application.Dtos.User;
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
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }
    [HttpPost("login")] 
    public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
    {
        var result = await accountService.LoginAsync(dto);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }
    [HttpPost("verify-login")]
    public async Task<IActionResult> VerifyAndLogin([FromBody] VerifyCodeDto dto)
    {
        var result = await accountService.VerifyAndLoginAsync(dto);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }
}
