using Startup_Project.Application.Dtos.Account;
using Startup_Project.Application.Dtos.Account.Password;
using Startup_Project.Application.Dtos.User;
using Startup_Project.Application.Models;

namespace Startup_Project.Application.Services.Interfaces.Account;

public interface IAccountService
{
    Task<ResponseModel<bool>> RegisterAsync(UserRegisterDto dto);
    Task<ResponseModel<string>> VerifyAndLoginAsync(VerifyCodeDto dto);
    Task<ResponseModel<string>> LoginAsync(UserLoginDto dto);
    Task<ResponseModel<string>> GoogleLoginAsync(GoogleLoginDto dto);
    Task<ResponseModel<bool>> ForgotPasswordAsync(string email);
    Task<ResponseModel<bool>> ResetPasswordAsync(ResetPasswordDto dto);

    // SMS OTP Methods
    Task<ResponseModel<bool>> SendSmsOtpAsync(SendSmsOtpDto dto);
    Task<ResponseModel<string>> VerifySmsOtpAsync(VerifySmsOtpDto dto);
}
