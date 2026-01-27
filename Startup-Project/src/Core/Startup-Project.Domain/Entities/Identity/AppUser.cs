using Microsoft.AspNetCore.Identity;
using Startup_Project.Domain.Enums;

namespace Startup_Project.Domain.Entities.Identity;

public class AppUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = null!;
    public List<Gym> Gyms { get; set; } = [];
    public string? VerificationCode { get; set; }
    public DateTime? CodeExpireDate { get; set; }
    public List<GymReview> Reviews { get; set; } = [];
    public string? GoogleId { get; set; }
    public AuthProvider AuthProvider { get; set; } = AuthProvider.Local;

    public AppUser() { }

    public AppUser(string fullName)
    {
        FullName = fullName;
    }

    public AppUser(string fullName, string googleId, AuthProvider authProvider)
    {
        FullName = fullName;
        GoogleId = googleId;
        AuthProvider = authProvider;
    }
}
