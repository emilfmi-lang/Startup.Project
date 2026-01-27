using Microsoft.AspNetCore.Mvc;
using Startup_Project.Application.Dtos.Account;
using Startup_Project.Application.Dtos.Account.Password;
using Startup_Project.Application.Dtos.User;
using Startup_Project.Application.Services.Interfaces.Account;

namespace Startup_Project.WebAPI.Controllers.Account;

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

    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto dto)
    {
        var result = await accountService.GoogleLoginAsync(dto);
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

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        var result = await accountService.ForgotPasswordAsync(dto.Email);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var result = await accountService.ResetPasswordAsync(dto);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }
}
