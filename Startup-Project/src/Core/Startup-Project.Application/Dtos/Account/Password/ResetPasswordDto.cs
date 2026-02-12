namespace Startup_Project.Application.Dtos.Account.Password;

public record ResetPasswordDto
{
    public string Email { get; init; }
    public string Code { get; init; }
    public string NewPassword { get; init; }
    public string ConfirmPassword { get; init; }
}
