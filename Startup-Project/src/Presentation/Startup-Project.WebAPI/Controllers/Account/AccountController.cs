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
        => Ok(await accountService.RegisterAsync(dto));

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
        => Ok(await accountService.LoginAsync(dto));

    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto dto)
        => Ok(await accountService.GoogleLoginAsync(dto));

    [HttpPost("verify-login")]
    public async Task<IActionResult> VerifyAndLogin([FromBody] VerifyCodeDto dto)
        => Ok(await accountService.VerifyAndLoginAsync(dto));

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        => Ok(await accountService.ForgotPasswordAsync(dto.Email));

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        => Ok(await accountService.ResetPasswordAsync(dto));

        [HttpPost("send-otp")]
        public async Task<IActionResult> SendSmsOtp([FromBody] SendSmsOtpDto dto)
            => Ok(await accountService.SendSmsOtpAsync(dto));

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifySmsOtp([FromBody] VerifySmsOtpDto dto)
            => Ok(await accountService.VerifySmsOtpAsync(dto));
    }
