namespace Startup_Project.Application.Dtos.Token;

public class RefreshTokenRequestDto
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
}
