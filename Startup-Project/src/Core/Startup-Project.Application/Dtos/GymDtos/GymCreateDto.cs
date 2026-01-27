using System.ComponentModel.DataAnnotations;

namespace Startup_Project.Application.Dtos.GymDtos;

public class GymCreateDto
{
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
}
