using Startup_Project.Domain.Entities.Common;
using Startup_Project.Domain.Entities.Identity;

namespace Startup_Project.Domain.Entities;

public class UserSession : BaseEntity
{
    public Guid UserId { get; private set; }
    public AppUser User { get; private set; } = null!;
    public string DeviceInfo { get; private set; } = null!;   
    public string? Location { get; private set; }             
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public string RefreshToken { get; private set; } = null!;
    public DateTime RefreshTokenExpiry { get; private set; }
    public bool IsCurrent { get; private set; }
    public DateTime LastActiveAt { get; private set; }
    private UserSession() { }

    public UserSession(
        Guid userId,
        string deviceInfo,
        string refreshToken,
        DateTime refreshTokenExpiry,
        string? location = null,
        string? ipAddress = null,
        string? userAgent = null)
    {
        UserId = userId;
        DeviceInfo = deviceInfo;
        RefreshToken = refreshToken;
        RefreshTokenExpiry = refreshTokenExpiry;
        Location = location;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        IsCurrent = false;
        LastActiveAt = DateTime.UtcNow;
    }
    public void MarkAsCurrent()
    {
        IsCurrent = true;
        LastActiveAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UnmarkAsCurrent()
    {
        IsCurrent = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateActivity()
    {
        LastActiveAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Revoke()
    {
        IsDeleted = true;
        IsCurrent = false;
        UpdatedAt = DateTime.UtcNow;
    }

}
