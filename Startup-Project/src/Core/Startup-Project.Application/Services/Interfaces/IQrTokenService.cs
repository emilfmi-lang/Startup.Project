using Startup_Project.Application.Models;

namespace Startup_Project.Application.Services.Interfaces;

public interface IQrTokenService
{
    Task<ResponseModel<string>> GenerateQrTokenAsync(Guid userId);
    Task<ResponseModel<bool>> VerifyQrTokenAsync(string token);
}
