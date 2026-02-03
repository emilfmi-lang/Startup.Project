using Startup_Project.Domain.Entities.Common;
using Startup_Project.Domain.Exceptions;

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
            throw BusinessException.ValidationFailed("Qiym?t 0-dan böyük olmal?d?r.");

        if (durationInDays <= 0)
            throw BusinessException.ValidationFailed("Müdd?t 0-dan böyük olmal?d?r.");

        Name = name;
        Price = price;
        DurationInDays = durationInDays;
        GymId = gymId;
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw BusinessException.ValidationFailed("Paket ad? bo? ola bilm?z.");
        Name = name;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetPrice(decimal price)
    {
        if (price <= 0)
            throw BusinessException.ValidationFailed("Qiym?t 0-dan böyük olmal?d?r.");
        Price = price;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetDurationInDays(int durationInDays)
    {
        if (durationInDays <= 0)
            throw BusinessException.ValidationFailed("Müdd?t 0-dan böyük olmal?d?r.");
        DurationInDays = durationInDays;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetGymId(Guid gymId)
    {
        GymId = gymId;
        UpdatedAt = DateTime.UtcNow;
    }
}
