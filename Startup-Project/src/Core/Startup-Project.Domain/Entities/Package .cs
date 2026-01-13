using Startup_Project.Domain.Entities.Common;

namespace Startup_Project.Domain.Entities;

public class Package: BaseEntity
{
    public string Name { get; private set; } = null!;
    public decimal Price { get; private set; }
    public int DurationInDays { get; private set; }
    public Guid GymId { get; private set; }
    public Gym Gym { get; private set; } = null!;
    private Package() { }
    public Package(
        string name,
        decimal price,
        int durationInDays,
        Guid gymId)
    {
        if (price <= 0)
            throw new Exception("Price must be positive");

        if (durationInDays <= 0)
            throw new Exception("Duration must be positive");

        Name = name;
        Price = price;
        DurationInDays = durationInDays;
        GymId = gymId;
    }
}
