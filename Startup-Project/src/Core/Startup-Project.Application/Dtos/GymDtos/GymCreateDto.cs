using System.ComponentModel.DataAnnotations;

namespace Startup_Project.Application.Dtos.GymDtos;

public class GymCreateDto
{
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string? Description { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
