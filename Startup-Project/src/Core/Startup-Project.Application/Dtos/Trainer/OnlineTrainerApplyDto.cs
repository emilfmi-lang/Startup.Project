namespace Startup_Project.Application.Dtos.Trainer;

public class OnlineTrainerApplyDto
{
    public string Specialization { get; set; } = null!;
    public string? Bio { get; set; }
    public decimal SessionPrice { get; set; }
    public string? CertificateUrl { get; set; }
    public string? VideoIntroUrl { get; set; }
    public string? SocialMediaLinks { get; set; }
}
