namespace Startup_Project.Application.Dtos.GymOwner;

public class MyGymDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string? ImagePath { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsOpen { get; set; }
}
