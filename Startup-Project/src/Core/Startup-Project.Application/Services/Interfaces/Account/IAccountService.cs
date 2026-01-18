using Startup_Project.Application.Dtos.Account;
using Startup_Project.Application.Models;

namespace Startup_Project.Application.Services.Interfaces.Account;

public interface IAccountService
{
    Task<ResponseModel<bool>> RegisterAsync(UserRegisterDto dto);
    Task<ResponseModel<string>> LoginAsync(UserLoginDto dto);
}
