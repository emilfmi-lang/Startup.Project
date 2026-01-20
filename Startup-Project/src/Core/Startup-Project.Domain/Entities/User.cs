
using Startup_Project.Domain.Entities.Common;

namespace Startup_Project.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string? VerificationCode { get; set; }
    public DateTime? CodeExpireDate { get; set; }
    public string FullName { get; set; }
}
