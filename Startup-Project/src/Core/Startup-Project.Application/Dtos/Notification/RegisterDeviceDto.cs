using Startup_Project.Domain.Enums.Device;

namespace Startup_Project.Application.Dtos.Notification;

public class RegisterDeviceDto
{
    public string DeviceToken { get; set; } = null!;
    public DevicePlatform Platform { get; set; }
    public string? DeviceModel { get; set; }
    public string? AppVersion { get; set; }
}
