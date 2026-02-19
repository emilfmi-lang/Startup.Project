using Startup_Project.Application.Dtos.Token;
using Startup_Project.Application.Models;

namespace Startup_Project.Application.Services.Interfaces.Account;

public interface IGoogleAuthService
{
    Task<ResponseModel<TokenDto>> GoogleLoginAsync(string idToken);
}
