namespace Startup_Project.Application.Dtos.Trainer;

public class TrainerListDto
{
    public Guid Id { get; set; }
    public string UserFullName { get; set; } = null!;
    public string Specialization { get; set; } = null!;
    public List<string> Specializations { get; set; } = [];
    public string? Bio { get; set; }
    public string? ProfileImageUrl { get; set; }
    public decimal SessionPrice { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public int TotalClients { get; set; }
    public int TotalSessions { get; set; }
    public string? GymName { get; set; }
    public bool IsOnlineTrainer { get; set; }
    public bool IsVerified { get; set; }
    public string Status { get; set; } = null!;
}
