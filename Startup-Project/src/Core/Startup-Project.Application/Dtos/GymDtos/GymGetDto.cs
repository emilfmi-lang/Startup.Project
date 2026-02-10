
namespace Startup_Project.Application.Dtos.GymDtos;

public class GymGetDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string OwnerFullName { get; set; } = null!;
    public string? ImagePath { get; set; }
    public bool IsFavorite { get; set; }
}
