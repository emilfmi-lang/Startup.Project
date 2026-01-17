

using Startup_Project.Domain.Entities.Identity;

namespace Startup_Project.Application.Abstractions.Services;

public interface IJwtTokenService
{
    string GenerateTokenAsync(AppUser user, IList<string> roles);
}
