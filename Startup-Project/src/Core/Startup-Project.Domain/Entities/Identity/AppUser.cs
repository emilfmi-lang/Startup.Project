using Microsoft.AspNetCore.Identity;

namespace Startup_Project.Domain.Entities.Identity;

public class AppUser:IdentityUser<Guid>
{
    public string FullName { get; set; } = null!;
    public List<Gym> Gyms { get; set; } = new List<Gym>();

    public AppUser() { }

    public AppUser(string fullName)
    {
        FullName = fullName;
    }
}
