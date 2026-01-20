using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Startup_Project.Application.Abstractions.Services;
using Startup_Project.Application.Dtos.User;

namespace Startup_Project.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("send-code")]
    public async Task<IActionResult> SendCode([FromBody] LoginStepOneDto loginStep)
    {
        await authService.SendVerificationCodeAsync(loginStep.Email);
        return Ok(new { Message = "Əgər email doğrudursa, kod göndərildi." });
    }
    [HttpPost("verify-code")]
    public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeDto verifyCodeDto)
    {
        var tokenDto = await authService.VerifyCodeAsync(verifyCodeDto);
        return Ok(tokenDto);
    }
}
