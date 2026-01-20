using Startup_Project.Application.Dtos.Token;
using Startup_Project.Application.Dtos.User;

namespace Startup_Project.Application.Abstractions.Services;

public interface IAuthService
{
    Task<bool> SendVerificationCodeAsync(string email); 
    Task<TokenDto> VerifyCodeAsync(VerifyCodeDto verifyCodeDto);
}
