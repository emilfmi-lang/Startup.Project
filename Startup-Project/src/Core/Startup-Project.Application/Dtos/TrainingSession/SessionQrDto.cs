

namespace Startup_Project.Application.Dtos.TrainingSession;

public class SessionQrDto
{
    public Guid SessionId { get; set; }
    public string QrToken { get; set; } = null!;
    public string TrainerName { get; set; } = null!;
    public string GymName { get; set; } = null!;
    public DateTime? ExpiresAt { get; set; }
    public string QrCodeBase64 { get; set; } = null!;
}
