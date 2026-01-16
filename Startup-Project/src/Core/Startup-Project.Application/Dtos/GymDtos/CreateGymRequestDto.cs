namespace Startup_Project.Application.Dtos.GymDtos;

public class CreateGymRequestDto
{
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public Guid OnwerId { get; init; }
}
