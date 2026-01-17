using Startup_Project.Domain.Entities.Common;
using Startup_Project.Domain.Entities.Identity;

namespace Startup_Project.Domain.Entities;

public class Gym:BaseEntity
{
    public string Name { get;private set; } = null!;
    public string Address { get;private set; } = null!;
    public Guid OwnerId { get;private set; }
    public AppUser Owner { get;private set; } = null!;
    private Gym() { }
    public Gym(string name, string address, Guid ownerId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new Exception("Gym name is required");
        Name = name;
        Address = address;
        OwnerId = ownerId;
    }
    public void ChangeAdress(string address)
    {
        Address = address;
        UpdatedAt = DateTime.UtcNow;

    }
}
