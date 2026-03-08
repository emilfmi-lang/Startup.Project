namespace Startup_Project.Application.Dtos.QrCode;

public class QrCodeResponseDto
{
    public string Token { get; set; } = null!;
    public string QrCodeBase64 { get; set; } = null!;
    public Guid GymId { get; set; }
    public string GymName { get; set; } = null!;
    public string? GymAddress { get; set; }
    public DateTime ExpiresAt { get; set; }
    public int ExpiresInSeconds { get; set; }
    public string SubscriptionPackageName { get; set; } = null!;
}
