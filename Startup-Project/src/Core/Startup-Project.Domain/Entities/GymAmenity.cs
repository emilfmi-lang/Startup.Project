using Startup_Project.Domain.Entities.Common;
using Startup_Project.Domain.Enums;

namespace Startup_Project.Domain.Entities;

public class GymAmenity : BaseEntity
{
    public Guid GymId { get;private set; }
    public Gym Gym { get; private set; } = null!;
    public AmenityType AmenityType { get; private set; }
    public string? CustomName { get; private set; }
    public string? IconName { get; private set; }
    public bool IsHighlighted { get; private set; }
    public int DisplayOrder { get; private set; }
    private GymAmenity() { }
    public GymAmenity(Guid gymId, AmenityType amenityType, bool isHighlighted = false, int displayOrder = 0)
    {
        GymId = gymId;
        AmenityType = amenityType;
        IsHighlighted = isHighlighted;
        DisplayOrder = displayOrder;
        CreatedAt = DateTime.UtcNow;
    }
    public void SetHighlighted(bool isHighlighted)
    {
        IsHighlighted = isHighlighted;
        UpdatedAt = DateTime.UtcNow;
    }
    public void SetDisplayOrder(int order)
    {
        DisplayOrder = order;
        UpdatedAt = DateTime.UtcNow;
    }
    public string GetDisplayName() => AmenityType.ToString();
}
