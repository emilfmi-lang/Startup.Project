namespace Startup_Project.Application.Dtos.GymOwner;

public class GymMemberDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PackageName { get; set; } = null!;
    public DateTime SubscriptionStartDate { get; set; }
    public DateTime SubscriptionEndDate { get; set; }
    public bool IsActive { get; set; }
}
