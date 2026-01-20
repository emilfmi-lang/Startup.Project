
using Startup_Project.Domain.Entities.Common;

namespace Startup_Project.Domain.Entities;

public class User : BaseEntity
{
    public string? VerificationCode { get; set; }
    public DateTime? CodeExpireDate { get; set; }
}
