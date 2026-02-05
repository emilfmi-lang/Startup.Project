using Startup_Project.Domain.Entities.Common;
using Startup_Project.Domain.Entities.Identity;
using Startup_Project.Domain.Enums.Device;

namespace Startup_Project.Domain.Entities;

public class UserDevice : BaseEntity
{
    public Guid UserId { get; private set; }
    public AppUser User { get; private set; } = null!;

    public string DeviceToken { get; private set; } = null!;
    public DevicePlatform Platform { get; private set; }
    public string? DeviceModel { get; private set; }
    public string? AppVersion { get; private set; }

    public bool IsActive { get; private set; } = true;
    public DateTime LastActiveAt { get; private set; }

    private UserDevice() { }

    public UserDevice(
        Guid userId,
        string deviceToken,
        DevicePlatform platform,
        string? deviceModel = null,
        string? appVersion = null)
    {
        UserId = userId;
        DeviceToken = deviceToken;
        Platform = platform;
        DeviceModel = deviceModel;
        AppVersion = appVersion;
        IsActive = true;
        LastActiveAt = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateToken(string newToken)
    {
        DeviceToken = newToken;
        LastActiveAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateActivity()
    {
        LastActiveAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateAppVersion(string appVersion)
    {
        AppVersion = appVersion;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        LastActiveAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
