namespace Startup_Project.Application.Abstractions.Services;

public interface IGoogleTokenValidator
{
    Task<GoogleUserInfo?> ValidateAsync(string idToken);
}

public class GoogleUserInfo
{
    public string GoogleId { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? PictureUrl { get; set; }
    public bool EmailVerified { get; set; }
}
