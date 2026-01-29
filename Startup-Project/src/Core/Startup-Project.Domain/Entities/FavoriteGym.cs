using Startup_Project.Domain.Entities.Common;
using Startup_Project.Domain.Entities.Identity;

namespace Startup_Project.Domain.Entities;

public class FavoriteGym : BaseEntity
{
    public Guid UserId { get; private set; }
    public AppUser User { get; private set; } = null!;

    public Guid GymId { get; private set; }
    public Gym Gym { get; private set; } = null!;

    private FavoriteGym() { }

    public FavoriteGym(Guid userId, Guid gymId)
    {
        UserId = userId;
        GymId = gymId;
        CreatedAt = DateTime.UtcNow;
    }
}
