using Startup_Project.Domain.Entities.Common;
using Startup_Project.Domain.Entities.Identity;

namespace Startup_Project.Domain.Entities;

public class Subscription: BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid PackageId { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public bool IsActive => DateTime.UtcNow <= EndDate;
    public AppUser User { get; private set; } = null!;
    public Package Package { get; private set; } = null!;
    private Subscription() { }
    public Subscription(Guid userId, Package package, DateTime startDate)
    {
        UserId = userId;
        PackageId = package.Id;
        StartDate = startDate;
        EndDate = startDate.AddDays(package.DurationInDays);
    }
    public void Extend(int extraDays)
    {
        if (extraDays <= 0)
            throw new ArgumentException("Extra days must be positive");

        EndDate = EndDate.AddDays(extraDays);
        UpdatedAt = DateTime.UtcNow;
    }
}
