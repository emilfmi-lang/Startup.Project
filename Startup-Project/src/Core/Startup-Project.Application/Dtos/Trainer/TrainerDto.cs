namespace Startup_Project.Application.Dtos.Trainer;

public class TrainerDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserFullName { get; set; } = null!;
    public Guid GymId { get; set; }
    public string GymName { get; set; } = null!;
    public string Specialization { get; set; } = null!;
    public string? Bio { get; set; }
    public string? CertificateUrl { get; set; }
    public string? ProfileImageUrl { get; set; }
    public decimal HourlyRate { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public int TotalClients { get; set; }
    public int TotalSessions { get; set; }
    public string Status { get; set; } = null!;
    public decimal SessionPrice { get; set; }
}
