using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Startup_Project.Domain.Entities.Common;
using Startup_Project.Domain.Entities.Identity;
using Startup_Project.Domain.Enums.Device;

namespace Startup_Project.Domain.Entities;

public class UserDevice : BaseEntity
{
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public string DeviceToken { get; set; } = null!;
    public DevicePlatform PlatForm { get; set; }
    public bool IsActive { get; set; } = true;

}
