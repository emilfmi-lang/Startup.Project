using Startup_Project.Domain.Entities.Common;

namespace Startup_Project.Domain.Entities;

public class GymImage : BaseEntity
{
    public Guid GymId { get; private set; }
    public Gym Gym { get; private set; } = null!;
    public string ImagePath { get; private set; } = null!;
    public string? Alt { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsMain { get; private set; }
    private GymImage() { }
    public GymImage(Guid gymId, string imagePath, int displayOrder = 0, bool isMain = false, string? alt = null)
    {
        GymId = gymId;
        ImagePath = imagePath;
        DisplayOrder = displayOrder;
        IsMain = isMain;
        Alt = alt;
        CreatedAt = DateTime.UtcNow;
    }
    public void SetAsMain()
    {
        IsMain = true;
        UpdatedAt = DateTime.UtcNow;
    }
    public void RemoveMain()
    {
        IsMain = false;
        UpdatedAt = DateTime.UtcNow;
    }
    public void SetDisplayOrder(int order)
    {
        DisplayOrder = order;
        UpdatedAt = DateTime.UtcNow;
    }

}
