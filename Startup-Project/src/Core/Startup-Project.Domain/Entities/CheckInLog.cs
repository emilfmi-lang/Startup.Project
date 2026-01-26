using Startup_Project.Domain.Entities.Common;
using Startup_Project.Domain.Entities.Identity;

namespace Startup_Project.Domain.Entities;

public class CheckInLog : BaseEntity
{
    public Guid UserId { get; set; }
    public AppUser User { get; set; }
    public DateTime CheckInTime { get; set; }
}
