namespace Startup_Project.Application.Dtos.QrCode;

public class QrVerifyResponseDto
{
    public bool IsSuccess { get; set; }
    public string UserFullName { get; set; } = null!;
    public string? UserProfileImage { get; set; }
    public string GymName { get; set; } = null!;
    public string PackageName { get; set; } = null!;
    public DateTime CheckInTime { get; set; }
    public DateTime SubscriptionEndDate { get; set; }
    public int DaysRemaining { get; set; }
}
