namespace Startup_Project.Application.Dtos.GymDtos;

public class GymAmenityDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? IconName { get; set; }
    public bool IsHighlighted { get; set; }
}
