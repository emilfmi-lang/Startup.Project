using Startup_Project.Application.Models;

namespace Startup_Project.Application.Services.Interfaces.Account;

/// <summary>
/// Google OAuth2 autentifikasiya servisi
/// JWT token qaytar?r
/// </summary>
public interface IGoogleAuthService
{
    /// <summary>
    /// Google ID Token-i do?rulay?b, JWT token qaytar?r
    /// </summary>
    /// <param name="idToken">Google t?r?find?n veril?n ID Token</param>
    /// <returns>JWT Access Token</returns>
    /// <exception cref="Startup_Project.Domain.Exceptions.BusinessException">Token validasiya x?tas?</exception>
    Task<ResponseModel<string>> GoogleLoginAsync(string idToken);
}
