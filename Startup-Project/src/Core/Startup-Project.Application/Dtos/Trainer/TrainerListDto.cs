namespace Startup_Project.Application.Dtos.Trainer;

public class TrainerListDto
{
    public Guid Id { get; set; }
    public string UserFullName { get; set; } = null!;
    public string Specialization { get; set; } = null!;
    public string? ProfileImageUrl { get; set; }
    public decimal SessionPrice { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public string GymName { get; set; } = null!;
}
