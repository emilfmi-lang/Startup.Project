namespace Startup_Project.Application.Dtos.Trainer;

public class TrainerCreateDto
{
    public Guid UserId { get; set; }        
    public string Specialization { get; set; } = null!;
    public string? Bio { get; set; }
    public decimal HourlyRate { get; set; }
    public decimal SessionPrice { get; set; }
}
