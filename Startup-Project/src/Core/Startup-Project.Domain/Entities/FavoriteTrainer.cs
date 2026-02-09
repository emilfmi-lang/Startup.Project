using Startup_Project.Domain.Entities.Common;
using Startup_Project.Domain.Entities.Identity;

namespace Startup_Project.Domain.Entities;

public class FavoriteTrainer : BaseEntity
{
    public Guid UserId { get; private set; }
    public AppUser User { get; private set; } = null!;
    public Guid TrainerId { get; private set; }
    public Trainer Trainer { get; private set; } = null!;
    private FavoriteTrainer() { }
    public FavoriteTrainer(Guid userId, Guid trainerId)
    {
        UserId = userId;
        TrainerId = trainerId;
        CreatedAt = DateTime.UtcNow;

    }
}
