using Startup_Project.Domain.Entities.Common;
using Startup_Project.Domain.Entities.Identity;

namespace Startup_Project.Domain.Entities;

public class GymReview:BaseEntity
{
    public string Comment { get; private set; }
    public int StarCount { get; private set; }
    public Guid GymId { get; private set; }
    public Gym Gym { get; private set; }    
    public Guid UserId { get; set; }
    public AppUser User { get; private set; }
    public GymReview()
    {
        
    }
        public GymReview(string comment, int starCount, Guid gymId, Guid userId)
        {
            if (starCount < 1 || starCount > 5) throw new ArgumentException("Ulduz 1-5 arası olmalıdır.");

            Comment = comment;
            StarCount = starCount;
            GymId = gymId;
            UserId = userId;
        }

        public void Reactivate(int starCount, string comment)
        {
            if (starCount < 1 || starCount > 5) throw new ArgumentException("Ulduz 1-5 arası olmalıdır.");

            StarCount = starCount;
            Comment = comment;
            IsDeleted = false;
            UpdatedAt = DateTime.UtcNow;
        }
    }
