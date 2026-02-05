using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Startup_Project.Domain.Entities.Common;
using Startup_Project.Domain.Entities.Identity;
using Startup_Project.Domain.Enums.Device;

namespace Startup_Project.Domain.Entities;

public class UserDevice : BaseEntity
{
    public Guid UserId { get; private set; }
    public AppUser User { get; private set; } = null!;
    public string DeviceToken { get; private set; } = null!;
    public DevicePlatform PlatForm { get; private set; }
    public bool IsActive { get; private set; } = true;

}
