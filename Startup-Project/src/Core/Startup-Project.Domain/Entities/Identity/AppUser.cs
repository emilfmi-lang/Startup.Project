using Microsoft.AspNetCore.Identity;

namespace Startup_Project.Domain.Entities.Identity;

public class AppUser:IdentityUser<Guid>
{
    public string FullName { get;private set; } = null!;
    private AppUser() { }
    public AppUser(string fullName)
    {
        FullName = fullName;
    }
}
