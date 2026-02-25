using Startup_Project.Domain.Entities.Common;
using Startup_Project.Domain.Entities.Identity;

namespace Startup_Project.Domain.Entities;

public class TrainerReview:BaseEntity
{
    public Guid TrainerId { get; private set; }
    public Trainer Trainer { get; private set; } = null!;

    public Guid UserId { get; private set; }
    public AppUser User { get; private set; } = null!;

    public int StarCount { get; private set; } 
    public string? Comment { get; private set; }
    private TrainerReview() { }

    public TrainerReview(Guid trainerId, Guid userId, int starCount, string? comment = null)
    {
        if (starCount < 1 || starCount > 5)
            throw new ArgumentException("Ulduz 1-5 arasi olmalidir.");

        TrainerId = trainerId;
        UserId = userId;
        StarCount = starCount;
        Comment = comment;
        CreatedAt = DateTime.UtcNow;
    }

        public void Update(int starCount, string? comment)
        {
            if (starCount < 1 || starCount > 5)
                throw new ArgumentException("Ulduz 1-5 arasi olmalidir.");

            StarCount = starCount;
            Comment = comment;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Reactivate(int starCount, string? comment)
        {
            if (starCount < 1 || starCount > 5)
                throw new ArgumentException("Ulduz 1-5 arasi olmalidir.");

            StarCount = starCount;
            Comment = comment;
            IsDeleted = false;
            UpdatedAt = DateTime.UtcNow;
        }
    }
