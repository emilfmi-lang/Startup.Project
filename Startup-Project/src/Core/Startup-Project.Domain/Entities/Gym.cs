using Startup_Project.Domain.Entities.Common;
using Startup_Project.Domain.Entities.Identity;

namespace Startup_Project.Domain.Entities;

public class Gym : BaseEntity
{
    public string Name { get; private set; } = null!;
    public string Address { get; private set; } = null!;
    public string? ImagePath { get; private set; }
    public List<GymReview> Reviews { get; private set; } = [];
    public double AverageRating {  get;  set; } = 0;
    public int ReviewCount { get; set; } = 0;
    public Guid OwnerId { get; private set; }
    public AppUser Owner { get; private set; } = null!;

    private Gym() { }

    public Gym(string name, string address, Guid ownerId, string? imagePath = null)
    {
        SetName(name);
        SetAddress(address);
        OwnerId = ownerId;
        ImagePath = imagePath;
        CreatedAt = DateTime.UtcNow;
    }
    public void AddReview(double rating)
    {
        var total = AverageRating * ReviewCount;
        ReviewCount++;
        AverageRating = (total + rating) / ReviewCount;
    }

    public void RemoveReview(double rating)
    {
        if (ReviewCount <= 0)
            return;

        var total = AverageRating * ReviewCount;
        ReviewCount--;

        if (ReviewCount == 0)
        {
            AverageRating = 0;
        }
        else
        {
            AverageRating = (total - rating) / ReviewCount;
        }
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Gym name cannot be empty.");
        Name = name;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Address cannot be empty.");
        Address = address;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateImage(string newPath)
    {
        ImagePath = newPath;
        UpdatedAt = DateTime.UtcNow;
    }
}
