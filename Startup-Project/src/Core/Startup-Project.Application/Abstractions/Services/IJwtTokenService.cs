

using System.Security.Claims;
using Startup_Project.Domain.Entities.Identity;

namespace Startup_Project.Application.Abstractions.Services;

public interface IJwtTokenService
{
    string GenerateAccessToken(AppUser user, IList<string> roles);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);

    // Geri uygunluq ucun — kohne metod
    string GenerateTokenAsync(AppUser user, IList<string> roles);
}
