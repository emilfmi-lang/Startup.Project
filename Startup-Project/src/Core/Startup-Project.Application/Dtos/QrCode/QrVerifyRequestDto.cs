namespace Startup_Project.Application.Dtos.QrCode;

public class QrVerifyRequestDto
{
    public string Token { get; set; } = null!;
    public Guid GymId { get; set; }
}
