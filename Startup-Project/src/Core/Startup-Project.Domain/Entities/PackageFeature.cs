using Startup_Project.Domain.Entities.Common;

namespace Startup_Project.Domain.Entities;

public class PackageFeature : BaseEntity
{
    public Guid PackageId { get; private set; }
    public Package Package { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public int DisplayOrder { get; private set; }
    private PackageFeature() { }

    public PackageFeature(Guid packageId, string name, int displayOrder = 0)
    {
        PackageId = packageId;
        Name = name;
        DisplayOrder = displayOrder;
        CreatedAt = DateTime.UtcNow;
    }

    public void SetName(string name)
    {
        Name = name;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetDisplayOrder(int order)
    {
        DisplayOrder = order;
        UpdatedAt = DateTime.UtcNow;
    }
}
