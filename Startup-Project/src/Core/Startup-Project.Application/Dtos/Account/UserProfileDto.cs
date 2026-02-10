namespace Startup_Project.Application.Dtos.Account;

public class UserProfileDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string? Bio { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string AuthProvider { get; set; } = null!;  // "Local" | "Google" | "Phone"
    public DateTime JoinedAt { get; set; }
}
