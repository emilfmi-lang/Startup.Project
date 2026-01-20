using Startup_Project.Application.Dtos.User;

namespace Startup_Project.Application.Abstractions.Services;

public interface IAuthService
{
    Task SendVerificationCodeAsync(string email); 
    Task<bool> VerifyCodeAsync(VerifyCodeDto verifyCodeDto);
}
