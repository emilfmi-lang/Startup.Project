namespace Startup_Project.Application.Dtos.GymOwner;

public class GymCheckInDto
{
    public Guid UserId { get; set; }
    public string UserFullName { get; set; } = null!;
    public DateTime CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
}
