namespace Startup_Project.Application.Dtos.Account;

public class VerifySmsOtpDto
{
    public string PhoneNumber { get; set; } = null!;
    public string Code { get; set; } = null!;
}
