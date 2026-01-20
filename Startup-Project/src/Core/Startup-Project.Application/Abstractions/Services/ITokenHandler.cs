using Startup_Project.Application.Dtos.Token;
using Startup_Project.Domain.Entities;
using Startup_Project.Domain.Entities.Identity;

namespace Startup_Project.Application.Abstractions.Services;

public interface ITokenHandler
{
    Task<TokenDto> CreateAccessToken(AppUser appUser);
}
