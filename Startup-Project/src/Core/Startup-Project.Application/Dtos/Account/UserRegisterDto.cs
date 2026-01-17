namespace Startup_Project.Application.Dtos.Account;

public sealed class UserRegisterDto
{
    public string UserName { get; init; } = null!;
    public string Password { get; init; } = null!;
    public string ConfirmPassword { get; init; } = null!;
    public string Email { get; init; } = null!; 
    public string FullName { get; init; } = null!;

}
