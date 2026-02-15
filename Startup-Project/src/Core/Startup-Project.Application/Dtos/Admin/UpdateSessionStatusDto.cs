using Startup_Project.Domain.Enums;

namespace Startup_Project.Application.Dtos.Admin;

public class UpdateSessionStatusDto
{
    public ScheduledSessionStatus Status { get; set; }
}
