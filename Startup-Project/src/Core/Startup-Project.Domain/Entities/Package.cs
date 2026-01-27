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

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Package name cannot be empty.");
        Name = name;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetPrice(decimal price)
    {
        if (price <= 0)
            throw new ArgumentException("Price must be positive.");
        Price = price;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetDurationInDays(int durationInDays)
    {
        if (durationInDays <= 0)
            throw new ArgumentException("Duration must be positive.");
        DurationInDays = durationInDays;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetGymId(Guid gymId)
    {
        GymId = gymId;
        UpdatedAt = DateTime.UtcNow;
    }
}
