
namespace Startup_Project.Application.Dtos.Account;

public sealed class UserLoginDto
{
    public string Email { get; init; } = null!;
    public string Password { get; init; } = null!;
}
