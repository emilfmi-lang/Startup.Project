namespace Startup_Project.Application.Dtos.Payment;

public class SubscriptionDto
{
    public string PackageName { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
}
